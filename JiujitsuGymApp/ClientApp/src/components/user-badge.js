import { LitElement, html, css } from 'lit';

export class UserBadge extends LitElement {
    static properties = {
        userName: { type: String, attribute: 'user-name' },
        userInitial: { type: String, attribute: 'user-initial' },
        beltRank: { type: String, attribute: 'belt-rank' }
    }

    // Map colors
    get beltStyles() {
        const config = {
            'White': { bg: '#ffffff', text: '#333333', border: '#cccccc' },
            'Grey': { bg: '#9e9e9e', text: '#ffffff', border: '#757575' },
            'Yellow': { bg: '#ffeb3b', text: '#333333', border: '#fbc02d' },
            'Orange': { bg: '#ff9800', text: '#ffffff', border: '#e65100' },
            'Green': { bg: '#4caf50', text: '#ffffff', border: '#2e7d32' },
            'Blue': { bg: '#2196f3', text: '#ffffff', border: '#1565c0' },
            'Purple': { bg: '#9c27b0', text: '#ffffff', border: '#6a1b9a' },
            'Brown': { bg: '#795548', text: '#ffffff', border: '#4e342e' },
            'Black': { bg: '#212121', text: '#ffffff', border: '#000000' }
        };

        // Fallback if the beltRank doesn't match
        return config[this.beltRank] || config['White'];
    }

    render() {
        const { bg, text, border } = this.beltStyles;

        return html`
          <div class="user-badge" style="--belt-bg: ${bg}; --belt-text: ${text}; --belt-border: ${border}">
            <div class="user-icon">${this.userInitial}</div>
            <span class="user-name">${this.userName}</span>
            <span class="belt-tag">${this.beltRank} belt</span>
          </div>
        `;
    }

    static styles = css`
        .user-badge {
            display: inline-flex;
            align-items: center;
            gap: 10px;
            padding: 6px 14px;
            border-radius: 25px;
            font-family: system-ui, -apple-system, sans-serif;
            
            /* Use the CSS variables set in render() */
            background-color: var(--belt-bg);
            color: var(--belt-text);
            border: 2px solid var(--belt-border);
            
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .user-icon {
            width: 28px;
            height: 28px;
            background: white;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-weight: bold;
            font-size: 14px;
            /* Icon text matches the belt color for a nice look */
            color: var(--belt-bg);
            border: 1px solid rgba(0,0,0,0.1);
        }

        .user-name {
            font-weight: 600;
            font-size: 15px;
        }

        .belt-tag {
            font-size: 11px;
            text-transform: uppercase;
            letter-spacing: 0.5px;
            padding-left: 8px;
            border-left: 1px solid rgba(0,0,0,0.2);
        }
    `;
}

customElements.define('user-badge', UserBadge);