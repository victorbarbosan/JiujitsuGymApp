import { LitElement, html } from 'lit';

class AppToast extends LitElement {
    static properties = {
        message:  { type: String },
        type:     { type: String }, // 'success' | 'danger' | 'warning'
        visible:  { state: true },
    };

    createRenderRoot() { return this; }

    constructor() {
        super();
        this.message = '';
        this.type = 'success';
        this.visible = false;
        this._timer = null;
    }

    show(message, type = 'success', duration = 3500) {
        this.message = message;
        this.type = type;
        this.visible = true;
        clearTimeout(this._timer);
        this._timer = setTimeout(() => this.visible = false, duration);
    }

    render() {
        if (!this.visible) return html``;

        const icons = { success: 'fa-circle-check', danger: 'fa-circle-xmark', warning: 'fa-triangle-exclamation' };

        return html`
<div style="position: fixed; bottom: 3rem; left: 50%; transform: translateX(-50%); z-index: 9999; min-width: 280px">
    <div class="toast show align-items-center text-bg-${this.type} border-0 shadow" role="alert">
        <div class="d-flex">
            <div class="toast-body d-flex align-items-center gap-2">
                <i class="fas ${icons[this.type] ?? icons.success}"></i>
                ${this.message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto"
                @click=${() => this.visible = false}>
            </button>
        </div>
    </div>
</div>`;
    }
}

customElements.define('app-toast', AppToast);