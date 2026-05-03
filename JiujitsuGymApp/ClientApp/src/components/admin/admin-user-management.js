import { LitElement, html } from 'lit';
import { loadMore, searchUsers } from './admin-controls.js';
import './create-user-modal.js';
import './admin-user-modal.js';
import '../shared/search-bar.js';
import '../shared/app-toast.js';
import './sort-header.js';

class AdminUserManagementTable extends LitElement {
    static properties = {
        initialUsers:   { type: Array, attribute: 'initial-users' },
        users:          { state: true },
        skip:           { state: true },
        isLoading:      { state: true },
        showModal:      { state: true },
        showUserModal:  { state: true },
        selectedUserId: { state: true },
        searchQuery:    { state: true },
        hasMore:        { state: true },
        sortBy:         { state: true },
        sortDir:        { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.initialUsers = [];
        this.users = [];
        this.skip = 50;
        this.isLoading = false;
        this.showModal = false;
        this.showUserModal = false;
        this.selectedUserId = null;
        this.searchQuery = '';
        this.hasMore = true;
        this.sortBy = 'name';
        this.sortDir = 'asc';
    }

    connectedCallback() {
        super.connectedCallback();
        if (this.initialUsers.length > 0) {
            this.users = [...this.initialUsers];
            this.hasMore = this.initialUsers.length === this.skip;
        }
    }

    _toast(message, type = 'success') {
        this.renderRoot.querySelector('app-toast')?.show(message, type);
    }

    async handleSearchChange(e) {
        this.searchQuery = e.detail.query;
        await searchUsers(this, this.searchQuery);
    }

    async handleSortChanged(e) {
        this.sortBy = e.detail.field;
        this.sortDir = e.detail.direction;
        await searchUsers(this, this.searchQuery, this.sortBy, this.sortDir);
    }

    handleUserCreated(e) {
        this.users = [e.detail.user, ...this.users];
        this.skip += 1;
        this.showModal = false;
        this._toast('User created successfully.');
    }

    handleUserUpdated(e) {
        this.users = this.users.map(u =>
            u.id === e.detail.user.id ? e.detail.user : u
        );
        this._toast(`${e.detail.user.firstName} ${e.detail.user.lastName} updated successfully.`);
    }

    handleRowClick(userId) {
        this.selectedUserId = userId;
        this.showUserModal = true;
    }

    async handleLoadMore() {
        await loadMore(this);
    }

    render() {
        return html`
        <app-toast></app-toast>

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
                        <th scope="col">
                            <sort-header
                                label="Name"
                                field="name"
                                currentField=${this.sortBy}
                                currentDir=${this.sortDir}
                                @sort-changed=${this.handleSortChanged}>
                            </sort-header>
                        </th>
                        <th scope="col">
                            <sort-header
                                label="Email"
                                field="email"
                                currentField=${this.sortBy}
                                currentDir=${this.sortDir}
                                @sort-changed=${this.handleSortChanged}>
                            </sort-header>
                        </th>
                        <th scope="col">Belt</th>
                    </tr>
                </thead>
                <tbody>
                    ${this.users.map(u => html`
                        <tr style="cursor:pointer" @click=${() => this.handleRowClick(u.id)}>
                            <td>${u.firstName} ${u.lastName}</td>
                            <td>${u.email}</td>
                            <td>${u.belt}</td>
                        </tr>
                    `)}
                </tbody>
            </table>
        </div>

        ${this.hasMore ? html`
            <button class="btn btn-primary mt-3"
                    @click=${this.handleLoadMore}
                    ?disabled=${this.isLoading}>
                ${this.isLoading ? 'Oss... Loading' : 'Load More Members'}
            </button>
        ` : ''}

        <create-user-modal
            ?open=${this.showModal}
            @user-created=${this.handleUserCreated}
            @modal-close=${() => this.showModal = false}>
        </create-user-modal>

        <admin-user-modal
            user-id=${this.selectedUserId ?? ''}
            ?open=${this.showUserModal}
            @user-updated=${this.handleUserUpdated}
            @modal-close=${() => this.showUserModal = false}>
        </admin-user-modal>`;
    }
}

customElements.define('admin-user-management-table', AdminUserManagementTable);