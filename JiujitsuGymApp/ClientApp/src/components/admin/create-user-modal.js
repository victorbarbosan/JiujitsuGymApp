import { LitElement, html } from 'lit';
import { BELT_OPTIONS, ROLE_OPTIONS, submitCreate } from './admin-controls.js';
import '../shared/app-modal.js';

export class CreateUserModal extends LitElement {
    createRenderRoot() {
        return this;
    }

    static properties = {
        open: { type: Boolean },
        isSubmitting: { state: true },
        formErrors: { state: true },
        form: { state: true }
    };

    constructor() {
        super();
        this.open = false;
        this.isSubmitting = false;
        this.formErrors = [];
        this.form = { firstName: '', lastName: '', email: '', phoneNumber: '', belt: 'White', password: '', role: 'Member' };
    }

    reset() {
        this.form = { firstName: '', lastName: '', email: '', phoneNumber: '', belt: 'White', password: '', role: 'Member' };
        this.formErrors = [];
    }

    close() {
        this.dispatchEvent(new CustomEvent('modal-close', { bubbles: true, composed: true }));
    }

    updateField(field, value) {
        this.form = { ...this.form, [field]: value };
    }

    async handleSubmit(e) {
        await submitCreate(this, e, (newUser) => {
            this.dispatchEvent(new CustomEvent('user-created', {
                detail: { user: newUser },
                bubbles: true,
                composed: true
            }));
            this.reset();
        });
    }

    renderContent() {
        return html`
            <form @submit=${(e) => this.handleSubmit(e)}>
                <div class="modal-body">
                    ${this.formErrors.length > 0 ? html`
                        <div class="alert alert-danger">
                            <ul class="mb-0">
                                ${this.formErrors.map(err => html`<li>${err}</li>`)}
                            </ul>
                        </div>
                    ` : ''}
                    <div class="row mb-3">
                        <div class="col">
                            <label class="form-label">First Name</label>
                            <input class="form-control" required
                                   .value=${this.form.firstName}
                                   @input=${(e) => this.updateField('firstName', e.target.value)} />
                        </div>
                        <div class="col">
                            <label class="form-label">Last Name</label>
                            <input class="form-control" required
                                   .value=${this.form.lastName}
                                   @input=${(e) => this.updateField('lastName', e.target.value)} />
                        </div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Email</label>
                        <input type="email" class="form-control" required
                               .value=${this.form.email}
                               @input=${(e) => this.updateField('email', e.target.value)} />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Phone Number <span class="text-muted">(optional)</span></label>
                        <input type="tel" class="form-control"
                               .value=${this.form.phoneNumber}
                               @input=${(e) => this.updateField('phoneNumber', e.target.value)} />
                    </div>
                    <div class="row mb-3">
                        <div class="col">
                            <label class="form-label">Belt</label>
                            <select class="form-select"
                                    .value=${this.form.belt}
                                    @change=${(e) => this.updateField('belt', e.target.value)}>
                                ${BELT_OPTIONS.map(b => html`<option value=${b} ?selected=${this.form.belt === b}>${b}</option>`)}
                            </select>
                        </div>
                        <div class="col">
                            <label class="form-label">Role</label>
                            <select class="form-select"
                                    .value=${this.form.role}
                                    @change=${(e) => this.updateField('role', e.target.value)}>
                                ${ROLE_OPTIONS.map(r => html`<option value=${r} ?selected=${this.form.role === r}>${r}</option>`)}
                            </select>
                        </div>
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Password</label>
                        <input type="password" class="form-control" required minlength="6"
                               .value=${this.form.password}
                               @input=${(e) => this.updateField('password', e.target.value)} />
                        <small class="form-text text-muted">Minimum 6 characters.</small>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary" @click=${() => this.close()}>Cancel</button>
                    <button type="submit" class="btn btn-primary" ?disabled=${this.isSubmitting}>
                        ${this.isSubmitting ? 'Creating...' : 'Create User'}
                    </button>
                </div>
            </form>
        `;
    }

    render() {
        return html`
            <app-modal
                title="Create New User"
                ?open=${this.open}
                .content=${this.renderContent.bind(this)}
                @modal-close=${() => this.close()}>
            </app-modal>
        `;
    }
}

customElements.define('create-user-modal', CreateUserModal);