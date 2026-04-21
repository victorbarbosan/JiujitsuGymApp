import { LitElement, html } from 'lit';

export const SortDir = Object.freeze({
    ASC: 'asc',
    DESC: 'desc',
});

class SortHeader extends LitElement {
    static properties = {
        label: { type: String },
        field: { type: String },
        currentField: { type: String },
        currentDir: { type: String },
    };

    createRenderRoot() {
        return this;
    }

    constructor() {
        super();
        this.label = '';
        this.field = '';
        this.currentField = '';
        this.currentDir = SortDir.ASC;
    }

    get isActive() {
        return this.field === this.currentField;
    }

    get _icon() {
        if (!this.isActive) return null;
        return this.currentDir === SortDir.ASC ? 'fa-sort-up' : 'fa-sort-down';
    }

    sortChange() {
        const newDir = this.isActive && this.currentDir === SortDir.ASC
            ? SortDir.DESC
            : SortDir.ASC;

        this.dispatchEvent(new CustomEvent('sort-changed', {
            detail: { field: this.field, direction: newDir },
            bubbles: true,
            composed: true,
        }));
    }

    render() {
        return html`
            <button @click=${this.sortChange} class="btn btn-link p-0 text-decoration-none text-reset">
                ${this.label}
                ${this._icon ? html`<i class="fas ${this._icon} ms-1"></i>` : ''}
            </button>
        `;
    }
}

customElements.define('sort-header', SortHeader);