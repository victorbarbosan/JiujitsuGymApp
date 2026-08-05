import { LitElement, html, css } from 'lit';

export class UserNav extends LitElement {
    static properties = {
        userName: { type: String, attribute: 'user-name' },
        userEmail: { type: String, attribute: 'user-email' },
        isAuthenticated: { type: Boolean, attribute: 'is-authenticated' },
        isOpen: { type: Boolean, state: true } 
    };

    constructor() {
        super();
        this.userName = 'Guest';
        this.userEmail = '';
        this.isAuthenticated = false;
        this.isOpen = false;
    }

    toggleDropdown() {
        this.isOpen = !this.isOpen;
    }

    closeDropdown() {
        this.isOpen = false;
    }

    handleItemClick(action) {
        this.closeDropdown();
        this.dispatchEvent(new CustomEvent('nav-action', {
            detail: { action },
            bubbles: true,
            composed: true
        }));
    }

    getInitials() {
        return this.userName
            .split(' ')
            .map(name => name[0])
            .join('')
            .toUpperCase()
            .slice(0, 2);
    }

    render() {
        if (!this.isAuthenticated) {
            return html`
        <div class="auth-links">
          <button class="dropdown-item" @click=${() => this.handleItemClick('login')}>
            Login
          </button>
          <button class="dropdown-item" @click=${() => this.handleItemClick('register')}>
            Register
          </button>
        </div>
      `;
        }

        return html`
      <div class="user-nav">
        <button class="dropdown-trigger" @click=${this.toggleDropdown}>
          <div class="user-avatar">${this.getInitials()}</div>
          <div class="user-info">
            <span>${this.userName}</span>
            <span class="user-email">${this.userEmail}</span>
          </div>
          <div class="icon">${this.isOpen ? '▲' : '▼'}</div>
        </button>
        
        <div class="dropdown-menu ${this.isOpen ? 'open' : ''}">
          <button class="dropdown-item" @click=${() => this.handleItemClick('profile')}>
            My Profile
          </button>
          <button class="dropdown-item" @click=${() => this.handleItemClick('edit-profile')}>
            Edit Profile
          </button>
          <button class="dropdown-item" @click=${() => this.handleItemClick('change-password')}>
            Change Password
          </button>
          <div class="dropdown-divider"></div>
          <button class="dropdown-item logout" @click=${() => this.handleItemClick('logout')}>
            Logout
          </button>
        </div>
      </div>
    `;
    }

    static styles = css`
    :host {
      display: block;
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
    }
    
    .user-nav {
      position: relative;
    }
    
    .dropdown-trigger {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 16px;
      background: transparent;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.2s ease;
      color: #495057;
      font-size: 14px;
      font-weight: 500;
    }
    
    .dropdown-trigger:hover {
      background: #f8f9fa;
      border-color: #adb5bd;
    }
    
    /* --avatar-surface, --avatar-shadow and --belt-text all come from site.css
       via the belt-* class the host carries. Custom properties cross the
       shadow boundary, so this rule needs no knowledge of which belt is which
       and the disc matches the dashboard one without duplicating the recipe.
       Fallbacks cover a host that sets no belt class. */
    .user-avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background: var(--avatar-surface, linear-gradient(140deg, #4d9bff, #0a58ca));
      color: var(--belt-text, #ffffff);
      box-shadow: var(--avatar-shadow, 0 2px 8px rgba(0, 0, 0, 0.18));
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      font-size: 14px;
      letter-spacing: 0.02em;
    }
    
    .dropdown-menu {
      position: absolute;
      top: 100%;
      right: 0;
      margin-top: 8px;
      background: white;
      border: 1px solid #dee2e6;
      border-radius: 8px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.1);
      min-width: 200px;
      z-index: 1000;
      opacity: 0;
      visibility: hidden;
      transform: translateY(-10px);
      transition: all 0.2s ease;
    }
    
    .dropdown-menu.open {
      opacity: 1;
      visibility: visible;
      transform: translateY(0);
    }
    
    .dropdown-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      color: #495057;
      text-decoration: none;
      cursor: pointer;
      transition: background 0.2s ease;
      border: none;
      background: none;
      width: 100%;
      text-align: left;
      font-size: 14px;
    }
    
    .dropdown-item:hover {
      background: #f8f9fa;
    }
    
    .dropdown-divider {
      height: 1px;
      background: #dee2e6;
      margin: 4px 0;
    }
    
    .dropdown-item.logout {
      color: #dc3545;
    }
    
    .dropdown-item.logout:hover {
      background: #fff5f5;
    }
    
    .icon {
      width: 16px;
      height: 16px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .user-info {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }
    
    .user-email {
      font-size: 12px;
      color: #6c757d;
    }
  `;
}

customElements.define('user-nav', UserNav);