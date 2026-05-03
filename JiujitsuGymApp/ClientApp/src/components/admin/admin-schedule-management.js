import { LitElement, html } from 'lit';
import '../shared/app-modal.js';

const DAYS = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'];

function getAntiForgeryToken() {
    return document.querySelector('input[name="__RequestVerificationToken"]')?.value ?? '';
}

class AdminScheduleManagement extends LitElement {
    static properties = {
        initialSchedules: { type: Array, attribute: 'initial-schedules' },
        schedules:    { state: true },
        teachers:     { state: true },
        showModal:    { state: true },
        form:         { state: true },
        formErrors:   { state: true },
        isSubmitting: { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.initialSchedules = [];
        this.schedules = [];
        this.teachers = [];
        this.showModal = false;
        this.form = this._emptyForm();
        this.formErrors = [];
        this.isSubmitting = false;
    }

    _emptyForm() {
        return { teacherId: '', location: '', dayOfWeek: 'Monday', timeOfDay: '07:00' };
    }

    connectedCallback() {
        super.connectedCallback();
        this.schedules = [...this.initialSchedules];
    }

    async _openModal() {
        this.form = this._emptyForm();
        this.formErrors = [];
        if (this.teachers.length === 0) {
            const res = await fetch('/Admin/GetTeachers');
            this.teachers = await res.json();
        }
        if (this.teachers.length > 0) this.form = { ...this.form, teacherId: this.teachers[0].id };
        this.showModal = true;
    }

    async _handleSubmit(e) {
        e.preventDefault();
        if (this.isSubmitting) return;
        this.isSubmitting = true;
        this.formErrors = [];

        try {
            const res = await fetch('/Admin/CreateSchedule', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(this.form)
            });

            if (res.ok) {
                const created = await res.json();
                this.schedules = [...this.schedules, created].sort((a, b) =>
                    DAYS.indexOf(a.dayOfWeek) - DAYS.indexOf(b.dayOfWeek) ||
                    a.timeOfDay.localeCompare(b.timeOfDay));
                this.showModal = false;
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

    async _handleDelete(id) {
        if (!confirm('Deactivate this recurring slot?')) return;
        await fetch(`/Admin/DeleteSchedule/${id}`, {
            method: 'DELETE',
            headers: { 'RequestVerificationToken': getAntiForgeryToken() }
        });
        this.schedules = this.schedules.filter(s => s.id !== id);
    }

    _renderModalContent() {
        return html`
        <form @submit=${this._handleSubmit}>
            <div class="modal-body">
                ${this.formErrors.length > 0 ? html`
                    <div class="alert alert-danger">
                        <ul class="mb-0">${this.formErrors.map(e => html`<li>${e}</li>`)}</ul>
                    </div>` : ''}

                <div class="mb-3">
                    <label class="form-label">Instructor</label>
                    <select class="form-select" required
                        .value=${this.form.teacherId}
                        @change=${e => this.form = { ...this.form, teacherId: e.target.value }}>
                        ${this.teachers.map(t => html`<option value=${t.id}>${t.name}</option>`)}
                    </select>
                </div>

                <div class="mb-3">
                    <label class="form-label">Location</label>
                    <input class="form-control" required maxlength="100"
                        .value=${this.form.location}
                        @input=${e => this.form = { ...this.form, location: e.target.value }}
                        placeholder="e.g. Cambridge" />
                </div>

                <div class="mb-3">
                    <label class="form-label">Day of Week</label>
                    <select class="form-select"
                        .value=${this.form.dayOfWeek}
                        @change=${e => this.form = { ...this.form, dayOfWeek: e.target.value }}>
                        ${DAYS.map(d => html`<option value=${d}>${d}</option>`)}
                    </select>
                </div>

                <div class="mb-3">
                    <label class="form-label">Time</label>
                    <input type="time" class="form-control"
                        .value=${this.form.timeOfDay}
                        @change=${e => this.form = { ...this.form, timeOfDay: e.target.value }} />
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary"
                    @click=${() => this.showModal = false}>Cancel</button>
                <button type="submit" class="btn btn-primary" ?disabled=${this.isSubmitting}>
                    ${this.isSubmitting ? 'Saving...' : 'Save Slot'}
                </button>
            </div>
        </form>`;
    }

    render() {
        const grouped = DAYS.reduce((acc, day) => {
            const slots = this.schedules.filter(s => s.dayOfWeek === day && s.isActive);
            if (slots.length > 0) acc[day] = slots;
            return acc;
        }, {});

        return html`
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="mb-0">Recurring Schedule</h5>
            <button class="btn btn-primary" @click=${this._openModal}>
                <i class="fas fa-plus me-1"></i> Add Slot
            </button>
        </div>

        ${Object.keys(grouped).length === 0 ? html`
            <p class="text-muted">No recurring slots configured yet.</p>` : ''}

        ${Object.entries(grouped).map(([day, slots]) => html`
            <div class="mb-3">
                <h6 class="fw-bold border-bottom pb-1">${day}</h6>
                <div class="list-group">
                    ${slots.map(s => html`
                        <div class="list-group-item d-flex justify-content-between align-items-center">
                            <div>
                                <i class="fas fa-clock me-2 text-muted"></i>
                                <strong>${s.timeOfDay}</strong>
                                <span class="ms-2 text-muted">${s.location}</span>
                                <span class="ms-2 badge bg-secondary">${s.teacherName}</span>
                            </div>
                            <button class="btn btn-sm btn-outline-danger"
                                @click=${() => this._handleDelete(s.id)}>
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>`)}
                </div>
            </div>`)}

        <app-modal
            title="Add Recurring Slot"
            ?open=${this.showModal}
            .content=${this._renderModalContent.bind(this)}
            @modal-close=${() => this.showModal = false}>
        </app-modal>`;
    }
}

customElements.define('admin-schedule-management', AdminScheduleManagement);