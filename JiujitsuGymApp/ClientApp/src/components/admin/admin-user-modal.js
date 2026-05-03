import { LitElement, html } from 'lit';
import { BELT_OPTIONS, ROLE_OPTIONS } from './admin-controls.js';
import '../shared/app-modal.js';

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

class AdminUserModal extends LitElement {
    static properties = {
        userId:       { type: String, attribute: 'user-id' },
        open:         { type: Boolean },
        user:         { state: true },
        form:         { state: true },
        formErrors:   { state: true },
        isLoading:    { state: true },
        isSubmitting: { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.userId = null;
        this.open = false;
        this.user = null;
        this.form = null;
        this.formErrors = [];
        this.isLoading = false;
        this.isSubmitting = false;
    }

    updated(changed) {
        if (changed.has('open') && this.open && this.userId) {
            this._fetchUser();
        }
    }

    async _fetchUser() {
        this.isLoading = true;
        this.formErrors = [];
        try {
            const res = await fetch(`/Admin/GetUser/${this.userId}`);
            if (!res.ok) throw new Error();
            const data = await res.json();
            this.user = data;
            this.form = {
                firstName:   data.firstName,
                lastName:    data.lastName,
                email:       data.email,
                phoneNumber: data.phoneNumber ?? '',
                belt:        data.belt,
                role:        data.role,
            };
        } catch {
            this.formErrors = ['Failed to load user data.'];
        } finally {
            this.isLoading = false;
        }
    }

    _close() {
        this.dispatchEvent(new CustomEvent('modal-close', { bubbles: true, composed: true }));
    }

    async _handleSubmit(e) {
        e.preventDefault();
        if (this.isSubmitting) return;
        this.isSubmitting = true;
        this.formErrors = [];

        try {
            const res = await fetch(`/Admin/UpdateUser/${this.userId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(this.form)
            });

            if (res.ok) {
                const updated = await res.json();
                this.dispatchEvent(new CustomEvent('user-updated', {
                    detail: { user: updated },
                    bubbles: true,
                    composed: true
                }));
                this._close();
            } else {
                const data = await res.json();
                this.formErrors = data.errors ?? ['An unexpected error occurred.'];
            }
        } catch {
            this.formErrors = ['Network error. Please try again.'];
        } finally {
            this.isSubmitting = false;
        }
    }

    _renderContent() {
        if (this.isLoading) return html`
            <div class="modal-body text-center py-5">
                <div class="spinner-border text-primary" role="status"></div>
            </div>`;

        if (!this.form) return html``;

        return html`
        <form @submit=${(e) => this._handleSubmit(e)}>
            <div class="modal-body">
                ${this.formErrors.length > 0 ? html`
                    <div class="alert alert-danger">
                        <ul class="mb-0">${this.formErrors.map(e => html`<li>${e}</li>`)}</ul>
                    </div>` : ''}

                <div class="row mb-3">
                    <div class="col">
                        <label class="form-label">First Name</label>
                        <input class="form-control" required
                            .value=${this.form.firstName}
                            @input=${e => this.form = { ...this.form, firstName: e.target.value }} />
                    </div>
                    <div class="col">
                        <label class="form-label">Last Name</label>
                        <input class="form-control" required
                            .value=${this.form.lastName}
                            @input=${e => this.form = { ...this.form, lastName: e.target.value }} />
                    </div>
                </div>

                <div class="mb-3">
                    <label class="form-label">Email</label>
                    <input type="email" class="form-control" required
                        .value=${this.form.email}
                        @input=${e => this.form = { ...this.form, email: e.target.value }} />
                </div>

                <div class="mb-3">
                    <label class="form-label">Phone Number <span class="text-muted">(optional)</span></label>
                    <input type="tel" class="form-control"
                        .value=${this.form.phoneNumber}
                        @input=${e => this.form = { ...this.form, phoneNumber: e.target.value }} />
                </div>

                <div class="row mb-3">
                    <div class="col">
                        <label class="form-label">Belt</label>
                        <select class="form-select"
                            .value=${this.form.belt}
                            @change=${e => this.form = { ...this.form, belt: e.target.value }}>
                            ${BELT_OPTIONS.map(b => html`
                                <option value=${b} ?selected=${this.form.belt === b}>${b}</option>`)}
                        </select>
                    </div>
                    <div class="col">
                        <label class="form-label">Role</label>
                        <select class="form-select"
                            .value=${this.form.role}
                            @change=${e => this.form = { ...this.form, role: e.target.value }}>
                            ${ROLE_OPTIONS.map(r => html`
                                <option value=${r} ?selected=${this.form.role === r}>${r}</option>`)}
                        </select>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-outline-secondary"
                    @click=${() => this._close()}>Cancel</button>
                <button type="submit" class="btn btn-primary" ?disabled=${this.isSubmitting}>
                    ${this.isSubmitting ? 'Saving...' : 'Save Changes'}
                </button>
            </div>
        </form>`;
    }

    render() {
        const title = this.user
            ? `${this.user.firstName} ${this.user.lastName}`
            : 'User Profile';

        return html`
        <app-modal
            title=${title}
            ?open=${this.open}
            .content=${this._renderContent.bind(this)}
            @modal-close=${this._close}>
        </app-modal>`;
    }
}

customElements.define('admin-user-modal', AdminUserModal);