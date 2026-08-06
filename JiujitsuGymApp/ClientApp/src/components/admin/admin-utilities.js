import { LitElement, html } from 'lit';
import '../shared/app-modal.js';
import '../shared/app-toast.js';

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

class AdminUtilities extends LitElement {
    static properties = {
        initialStatus: { type: Object, attribute: 'initial-status' },
        status:        { state: true },
        isSeeding:     { state: true },
        isPurging:     { state: true },
        showConfirm:   { state: true },
        isStale:       { state: true },
        errors:        { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.initialStatus = {};
        this.status = {};
        this.isSeeding = false;
        this.isPurging = false;
        this.showConfirm = false;
        // Seeding and purging both change what the Users and Classes tabs
        // rendered at page load, and neither of those re-fetches on its own.
        this.isStale = false;
        this.errors = [];
    }

    connectedCallback() {
        super.connectedCallback();
        this.status = { ...this.initialStatus };
    }

    get _isBusy() { return this.isSeeding || this.isPurging; }

    _toast(message, type = 'success') {
        this.renderRoot.querySelector('app-toast')?.show(message, type);
    }

    async _handleSeed() {
        if (this._isBusy) return;
        this.isSeeding = true;
        this.errors = [];

        try {
            const res = await fetch('/Admin/SeedDemoData', {
                method: 'POST',
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });

            if (res.ok) {
                const result = await res.json();
                this.status = result.status;
                this.isStale = true;
                this._toast(result.message);
            } else {
                const data = await res.json();
                this.errors = data.errors ?? ['An unexpected error occurred.'];
                this._toast('Could not seed demo data.', 'danger');
            }
        } catch {
            this.errors = ['Network error. Please try again.'];
            this._toast('Could not seed demo data.', 'danger');
        } finally {
            this.isSeeding = false;
        }
    }

    async _handlePurge() {
        if (this._isBusy) return;
        this.isPurging = true;
        this.showConfirm = false;
        this.errors = [];

        try {
            const res = await fetch('/Admin/PurgeDemoData', {
                method: 'DELETE',
                headers: { 'RequestVerificationToken': getAntiForgeryToken() }
            });

            if (res.ok) {
                const result = await res.json();
                this.status = result.status;
                this.isStale = true;
                this._toast(result.message, 'warning');
            } else {
                this.errors = ['Could not remove the demo data.'];
                this._toast('Could not remove demo data.', 'danger');
            }
        } catch {
            this.errors = ['Network error. Please try again.'];
            this._toast('Could not remove demo data.', 'danger');
        } finally {
            this.isPurging = false;
        }
    }

    _renderCounts() {
        const rows = [
            ['Instructors', this.status.teachers],
            ['Members', this.status.members],
            ['Recurring slots', this.status.schedules],
            ['Classes', this.status.classes],
            ['Check-ins', this.status.attendances],
            ['Shop items', this.status.products],
        ];

        return html`
        <div class="row g-2 mb-3">
            ${rows.map(([label, value]) => html`
                <div class="col-6 col-md-4 col-lg-2">
                    <div class="border rounded p-2 text-center h-100">
                        <div class="fs-4 fw-bold">${value ?? 0}</div>
                        <div class="small text-muted">${label}</div>
                    </div>
                </div>`)}
        </div>`;
    }

    // Rendered by app-modal, not by this component, so every handler below has
    // to be an arrow function: Lit invokes a bare method reference with the
    // rendering host as `this`, which here would be the modal.
    _renderConfirmContent() {
        return html`
        <div class="modal-body">
            <p>This permanently deletes every seeded account and everything hanging off it:</p>
            <ul>
                <li><strong>${(this.status.teachers ?? 0) + (this.status.members ?? 0)}</strong> demo accounts</li>
                <li><strong>${this.status.schedules ?? 0}</strong> recurring slots and
                    <strong>${this.status.classes ?? 0}</strong> classes</li>
                <li><strong>${this.status.attendances ?? 0}</strong> check-ins</li>
                <li><strong>${this.status.products ?? 0}</strong> shop items</li>
            </ul>
            <p class="mb-0 text-muted small">
                Only rows owned by an <code>@${this.status.demoEmailDomain}</code> account are touched.
                Real members, their classes and their attendance history are left alone.
            </p>
        </div>
        <div class="modal-footer">
            <button type="button" class="btn btn-secondary"
                @click=${() => this.showConfirm = false}>Cancel</button>
            <button type="button" class="btn btn-danger" @click=${() => this._handlePurge()}>
                Delete demo data
            </button>
        </div>`;
    }

    render() {
        const seeded = this.status.isSeeded === true;

        return html`
        <app-toast></app-toast>

        <h5 class="mb-1">Demo Data</h5>
        <p class="text-muted">
            Populates the site with a fictional academy - instructors, members across
            every belt, a weekly timetable, several weeks of past and upcoming classes,
            attendance history and a few shop items.
        </p>

        ${this.isStale ? html`
            <div class="alert alert-info d-flex justify-content-between align-items-center">
                <span>The Users and Classes tabs still show what was loaded before this change.</span>
                <button class="btn btn-sm btn-outline-primary" @click=${() => location.reload()}>
                    Reload
                </button>
            </div>` : ''}

        ${this.errors.length > 0 ? html`
            <div class="alert alert-danger">
                <ul class="mb-0">${this.errors.map(e => html`<li>${e}</li>`)}</ul>
            </div>` : ''}

        ${seeded ? this._renderCounts() : html`
            <p class="text-muted fst-italic">No demo data in the database.</p>`}

        ${seeded && this.status.demoPassword ? html`
            <p class="small text-muted">
                Every seeded account signs in with
                <code>${this.status.demoPassword}</code> - for example
                <code>marco.ferreira@${this.status.demoEmailDomain}</code> for an instructor.
            </p>` : ''}

        <div class="d-flex gap-2">
            <button class="btn btn-primary"
                ?disabled=${this._isBusy || seeded}
                @click=${() => this._handleSeed()}>
                ${this.isSeeding
                    ? html`<span class="spinner-border spinner-border-sm me-1"></span> Seeding...`
                    : html`<i class="fas fa-database me-1"></i> Seed demo data`}
            </button>

            <button class="btn btn-outline-danger"
                ?disabled=${this._isBusy || !seeded}
                @click=${() => this.showConfirm = true}>
                ${this.isPurging
                    ? html`<span class="spinner-border spinner-border-sm me-1"></span> Removing...`
                    : html`<i class="fas fa-trash me-1"></i> Delete demo data`}
            </button>
        </div>

        ${seeded ? '' : html`
            <p class="small text-muted mt-2 mb-0">
                Seeding writes around 600 rows, so it takes a few seconds.
            </p>`}

        <app-modal
            title="Delete all demo data?"
            ?open=${this.showConfirm}
            .content=${this._renderConfirmContent.bind(this)}
            @modal-close=${() => this.showConfirm = false}>
        </app-modal>`;
    }
}

customElements.define('admin-utilities', AdminUtilities);
