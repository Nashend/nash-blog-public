
document.addEventListener("DOMContentLoaded", () => {
    const topNav = document.getElementsByClassName("topnav")[0];
    if (topNav) {
        window.onscroll = () => {
            if (window.scrollY > 50) {
                // add classes
                topNav.classList.add('scrollednav', 'py-0');
            }
            else {
                // remove classes
                topNav.classList.remove('scrollednav', 'py-0');
            }
        }
    }
});

function toggleMenu(e) {
    e.target.classList.toggle('collapsed');
    document.getElementById("top-navbar-menus-wrapper").classList.toggle('show');
}