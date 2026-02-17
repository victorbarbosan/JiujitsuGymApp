import { LitElement, html, css } from 'lit';
import { customElement, property, state } from 'lit/decorators.js';
import { User } from '../../types/user'

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
        // initialize the state
        if (this.initialUsers.length > 0) {
            this.users = [...this.initialUsers]
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

    render() {
        return html`
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
    `;
    }



}