import { LitElement, html } from 'lit';
import { loadMore, searchUsers } from './admin-controls.js';
import './create-user-modal.js';
import '../shared/search-bar.js';

class AdminUserManagementTable extends LitElement {
    static properties = {
        initialUsers: { type: Array, attribute: 'initial-users' },
        users: { state: true },
        skip: { state: true },
        isLoading: { state: true },
        showModal: { state: true },
        searchQuery: { state: true },
    };

    createRenderRoot() {
        return this;
    }

    constructor() {
        super();
        this.initialUsers = [];
        this.users = [];
        this.skip = 50;
        this.isLoading = false;
        this.showModal = false;
        this.searchQuery = '';
    }

    connectedCallback() {
        super.connectedCallback();
        if (this.initialUsers.length > 0) {
            this.users = [...this.initialUsers];
        }
    }

    async handleSearchChange(e) {
        this.searchQuery = e.detail.query;
        await searchUsers(this, this.searchQuery);
    }

    handleUserCreated(e) {
        this.users = [e.detail.user, ...this.users];
        this.skip += 1;
        this.showModal = false;
    }

    async handleLoadMore() {
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

        <div class="mb-3">
            <search-bar
                placeholder="Search by name, email or belt..."
                @search-change=${this.handleSearchChange}>
            </search-bar>
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

customElements.define('admin-user-management-table', AdminUserManagementTable);