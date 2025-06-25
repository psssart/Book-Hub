class LayoutHeader extends HTMLElement {
    constructor() {
super();
const shadow = this.attachShadow({ mode: 'open' });

    shadow.innerHTML = `
<header>
    <div class="header">
        <div class="box">
            <div class="text-boxes">
                <div class="text-box">
                    <a href="../../registration.html">
                        <div class="text-wrapper-yellow">
                            <div class="text">
                                Sign up
                            </div>
                        </div>
                    </a>
                </div>
                <div class="text-box">
                    <a href="../../authorisation.html">
                        <div class="text-wrapper-blue">
                            <div class="text">
                                Login in
                            </div>
                        </div>
                    </a>
                </div>
            </div>
        </div>
    </div>
</header>
`;
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = '/css/style.css';
        shadow.appendChild(link);
    }
}

customElements.define('layout-header', LayoutHeader);