import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { User } from '../../types/user';
import { loadMore } from './admin-controls.js';
import './create-user-modal.js';

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

    static styles = css`
    :host { display: block; }
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

    private handleUserCreated(e: CustomEvent) {
        this.users = [e.detail.user, ...this.users];
        this.skip += 1;
        this.showModal = false;
    }

    private async handleLoadMore() {
        await loadMore(this);
    }

    render() {
        return html`
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h5 class="mb-0">Members</h5>
            <button class="btn btn-primary" @click=${() => this.showModal = true}>
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
                @click=${this.handleLoadMore}
                ?disabled=${this.isLoading}>
            ${this.isLoading ? 'Oss... Loading' : 'Load More Members'}
        </button>

        <create-user-modal
            ?open=${this.showModal}
            @user-created=${this.handleUserCreated}
            @modal-close=${() => this.showModal = false}>
        </create-user-modal>
        `;
    }
}