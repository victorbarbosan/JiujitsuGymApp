import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { User } from '../../types/user'

const BELT_OPTIONS = ['White', 'Grey', 'Yellow', 'Orange', 'Green', 'Blue', 'Purple', 'Brown', 'Black'];
const ROLE_OPTIONS = ['Member', 'Teacher', 'Admin'];

function getAntiForgeryToken(): string {
    return (document.querySelector('input[name="__RequestVerificationToken"]') as HTMLInputElement)?.value ?? '';
}

@customElement('admin-user-management-table')
export class AdminUserManagementTable extends LitElement {
    createRenderRoot() {
        return this;
    }

    @property({ type: Array, attribute: 'initial-users' })
    initialUsers: User[] = [];

    @state()
    private users: User[] = [];
    @state()
    private skip: number = 50;
    @state()
    private isLoading: boolean = false;
    @state()
    private showModal: boolean = false;
    @state()
    private isSubmitting: boolean = false;
    @state()
    private formErrors: string[] = [];
    @state()
    private form = { firstName: '', lastName: '', email: '', phoneNumber: '', belt: 'White', password: '', role: 'Member' };

    static styles = css`
    :host { display : block}
    .btn-primary {
        background-color: var(--color-brand-primary, #4958ff);
        color: var(--color-text-inverse, #ffffff);
        padding: var(--space-2) var(--space-3);
        border-radius: var(--radius-md);
        cursor: pointer;
        border: none;
        transition: var(--transition-fast);
    }
    .btn-primary:disabled { opacity: 0.5; cursor: not-allowed; }
    `;

    connectedCallback() {
        super.connectedCallback();
        if (this.initialUsers.length > 0) {
            this.users = [...this.initialUsers];
        }
    }

    async loadMore() {
        if (this.isLoading) return;
        this.isLoading = true;
        try {
            const response = await fetch(`/Admin/GetUsers?skip=${this.skip}`);
            if (!response.ok) throw new Error('Failed to fetch');
            const newUsers: User[] = await response.json();
            this.users = [...this.users, ...newUsers];
            this.skip += newUsers.length;
        } catch (error) {
            console.error("Member Load Error:", error);
        } finally {
            this.isLoading = false;
        }
    }

    private openModal() {
        this.form = { firstName: '', lastName: '', email: '', phoneNumber: '', belt: 'White', password: '', role: 'Member' };
        this.formErrors = [];
        this.showModal = true;
    }

    private closeModal() {
        this.showModal = false;
    }

    private updateField(field: keyof typeof this.form, value: string) {
        this.form = { ...this.form, [field]: value };
    }

    async submitCreate(e: Event) {
        e.preventDefault();
        if (this.isSubmitting) return;
        this.isSubmitting = true;
        this.formErrors = [];

        try {
            const response = await fetch('/Admin/CreateUser', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify(this.form)
            });

            if (response.ok) {
                const newUser: User = await response.json();
                this.users = [newUser, ...this.users];
                this.skip += 1;
                this.closeModal();
            } else {
                const data = await response.json();
                this.formErrors = data.errors ?? ['An unexpected error occurred.'];
            }
        } catch {
            this.formErrors = ['Network error. Please try again.'];
        } finally {
            this.isSubmitting = false;
        }
    }

    render() {
        return html`
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="mb-0">Members</h5>
            <button class="btn btn-primary" @click=${this.openModal}>
                <i class="fas fa-user-plus me-1"></i> Create User
            </button>
        </div>

        <div class="table-responsive">
            <table class="table table-hover align-middle">
                <thead class="border-bottom">
                    <tr>
                        <th scope="col">Name</th>
                        <th scope="col">Email</th>
                        <th scope="col">Belt</th>
                    </tr>
                </thead>
                <tbody>
                    ${this.users.map(u => html`
                        <tr>
                            <td>${u.firstName} ${u.lastName}</td>
                            <td>${u.email}</td>
                            <td>${u.belt}</td>
                        </tr>
                    `)}
                </tbody>
            </table>
        </div>

        <button class="btn btn-primary mt-3"
                @click=${this.loadMore}
                ?disabled=${this.isLoading}>
            ${this.isLoading ? 'Oss... Loading' : 'Load More Members'}
        </button>

        ${this.showModal ? html`
        <div class="modal d-block" tabindex="-1" style="background:rgba(0,0,0,0.5);">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">Create New User</h5>
                        <button type="button" class="btn-close" @click=${this.closeModal}></button>
                    </div>
                    <form @submit=${this.submitCreate}>
                        <div class="modal-body">
                            ${this.formErrors.length > 0 ? html`
                                <div class="alert alert-danger">
                                    <ul class="mb-0">
                                        ${this.formErrors.map(e => html`<li>${e}</li>`)}
                                    </ul>
                                </div>
                            ` : ''}
                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">First Name</label>
                                    <input class="form-control" required
                                           .value=${this.form.firstName}
                                           @input=${(e: InputEvent) => this.updateField('firstName', (e.target as HTMLInputElement).value)} />
                                </div>
                                <div class="col">
                                    <label class="form-label">Last Name</label>
                                    <input class="form-control" required
                                           .value=${this.form.lastName}
                                           @input=${(e: InputEvent) => this.updateField('lastName', (e.target as HTMLInputElement).value)} />
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Email</label>
                                <input type="email" class="form-control" required
                                       .value=${this.form.email}
                                       @input=${(e: InputEvent) => this.updateField('email', (e.target as HTMLInputElement).value)} />
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Phone Number <span class="text-muted">(optional)</span></label>
                                <input type="tel" class="form-control"
                                       .value=${this.form.phoneNumber}
                                       @input=${(e: InputEvent) => this.updateField('phoneNumber', (e.target as HTMLInputElement).value)} />
                            </div>
                            <div class="row mb-3">
                                <div class="col">
                                    <label class="form-label">Belt</label>
                                    <select class="form-select"
                                            .value=${this.form.belt}
                                            @change=${(e: Event) => this.updateField('belt', (e.target as HTMLSelectElement).value)}>
                                        ${BELT_OPTIONS.map(b => html`<option value=${b} ?selected=${this.form.belt === b}>${b}</option>`)}
                                    </select>
                                </div>
                                <div class="col">
                                    <label class="form-label">Role</label>
                                    <select class="form-select"
                                            .value=${this.form.role}
                                            @change=${(e: Event) => this.updateField('role', (e.target as HTMLSelectElement).value)}>
                                        ${ROLE_OPTIONS.map(r => html`<option value=${r} ?selected=${this.form.role === r}>${r}</option>`)}
                                    </select>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Password</label>
                                <input type="password" class="form-control" required minlength="6"
                                       .value=${this.form.password}
                                       @input=${(e: InputEvent) => this.updateField('password', (e.target as HTMLInputElement).value)} />
                                <small class="form-text text-muted">Minimum 6 characters.</small>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-outline-secondary" @click=${this.closeModal}>Cancel</button>
                            <button type="submit" class="btn btn-primary" ?disabled=${this.isSubmitting}>
                                ${this.isSubmitting ? 'Creating...' : 'Create User'}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
        ` : ''}
    `;
    }
}