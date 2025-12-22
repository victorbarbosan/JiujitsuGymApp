import { LitElement, html, css } from 'https://cdn.jsdelivr.net/gh/lit/dist@2/all/lit-all.min.js';

export class ProfileCard extends LitElement {
    static properties = {
        firstName: { type: String, attribute: 'first-name' },
        lastName: { type: String, attribute: 'last-name' },
        email: { type: String },
        phoneNumber: { type: String, attribute: 'phone-number' },
        belt: { type: String },
        memberSince: { type: String, attribute: 'member-since' },
        lastLogin: { type: String, attribute: 'last-login' }
    };

    constructor() {
        super();
        this.firstName = '';
        this.lastName = '';
        this.email = '';
        this.phoneNumber = '';
        this.belt = 'White';
        this.memberSince = '';
        this.lastLogin = '';
    }

    // Belt Color Mapping
    get beltConfig() {
        const rank = this.belt ? this.belt.charAt(0).toUpperCase() + this.belt.slice(1).toLowerCase() : 'White';

        const config = {
            'White': { main: '#ffffff', text: '#333333', accent: '#667eea' },
            'Grey': { main: '#9e9e9e', text: '#ffffff', accent: '#757575' },
            'Yellow': { main: '#ffeb3b', text: '#333333', accent: '#fbc02d' },
            'Orange': { main: '#ff9800', text: '#ffffff', accent: '#ff9800' },
            'Green': { main: '#4caf50', text: '#ffffff', accent: '#4caf50' },
            'Blue': { main: '#2196f3', text: '#ffffff', accent: '#2196f3' },
            'Purple': { main: '#9c27b0', text: '#ffffff', accent: '#9c27b0' },
            'Brown': { main: '#795548', text: '#ffffff', accent: '#795548' },
            'Black': { main: '#212121', text: '#ffffff', accent: '#212121' }
        };
        return config[rank] || config['White'];
    }

    getInitial() {
        return this.firstName ? this.firstName[0].toUpperCase() : '?';
    }

    getFullName() {
        return `${this.firstName} ${this.lastName}`;
    }

    formatDate(dateString) {
        if (!dateString || dateString === 'Never') return 'Never';
        const date = new Date(dateString);
        return isNaN(date) ? dateString : date.toLocaleDateString('en-US', {
            year: 'numeric', month: 'long', day: 'numeric'
        });
    }

    render() {
        const { main, text, accent } = this.beltConfig;

        return html`
      <div class="profile-card" style="--belt-color: ${main}; --belt-text: ${text}; --avatar-accent: ${accent}">
        <div class="profile-header">
          <div class="profile-avatar">${this.getInitial()}</div>
          <h2 class="profile-name">${this.getFullName()}</h2>
          ${this.belt ? html`<div class="profile-belt">${this.belt} Belt</div>` : ''}
        </div>
        
        <div class="profile-body">
          <div class="info-grid">
            <div class="info-item">
              <span class="info-label">Email</span>
              <span class="info-value">${this.email}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Phone</span>
              <span class="info-value">${this.phoneNumber || 'Not provided'}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Member Since</span>
              <span class="info-value">${this.formatDate(this.memberSince)}</span>
            </div>
            <div class="info-item">
              <span class="info-label">Last Login</span>
              <span class="info-value">${this.formatDate(this.lastLogin)}</span>
            </div>
          </div>
          
          <div class="action-buttons">
            <button class="btn btn-primary" @click=${() => this.dispatchEvent(new CustomEvent('edit-profile'))}>
              Edit Profile
            </button>
            <button class="btn btn-outline" @click=${() => this.dispatchEvent(new CustomEvent('change-password'))}>
              Change Password
            </button>
          </div>
        </div>
      </div>
    `;
    }

    static styles = css`
    :host { display: block; margin: 20px 0; }
    .profile-card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.08);
      overflow: hidden;
      border: 1px solid #e9ecef;
    }
    .profile-header {
      /* Dynamic Gradient based on belt color */
      background: linear-gradient(135deg, var(--belt-color) 0%, #444 150%);
      color: var(--belt-text);
      padding: 32px 24px;
      text-align: center;
      transition: all 0.5s ease;
    }
    /* Special case for white belt header visibility */
    .profile-card[style*="--belt-color: #ffffff"] .profile-header {
        background: #f8f9fa;
        border-bottom: 1px solid #dee2e6;
    }
    .profile-avatar {
      width: 80px; height: 80px;
      border-radius: 50%;
      background: white;
      margin: 0 auto 16px;
      display: flex; align-items: center; justify-content: center;
      color: var(--avatar-accent);
      font-size: 32px; font-weight: bold;
      border: 4px solid rgba(0,0,0,0.1);
    }
    .profile-name { font-size: 24px; margin: 0 0 8px 0; }
    .profile-belt {
      display: inline-block;
      background: rgba(0,0,0,0.1);
      padding: 4px 16px;
      border-radius: 20px;
      font-size: 13px; font-weight: bold;
      text-transform: uppercase;
    }
    .profile-body { padding: 24px; }
    .info-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .info-label { font-size: 11px; color: #6c757d; font-weight: bold; text-transform: uppercase; }
    .info-value { display: block; font-size: 15px; color: #212529; }
    .action-buttons { display: flex; gap: 12px; margin-top: 24px; }
    .btn { flex: 1; padding: 12px; border-radius: 8px; border: none; font-weight: 600; cursor: pointer; }
    .btn-primary { background: #667eea; color: white; }
    .btn-outline { background: transparent; color: #667eea; border: 2px solid #667eea; }
  `;
}
customElements.define('profile-card', ProfileCard);