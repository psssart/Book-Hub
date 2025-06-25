document.addEventListener('DOMContentLoaded', () => {
    const titles = document.querySelectorAll('.book-title');

    titles.forEach(title => {
        let fontSize = 32;

        const parentWidth = title.parentElement.clientWidth;
        console.log(parentWidth)
        console.log(title.scrollWidth)
        console.log("---------------")
        title.style.fontSize = fontSize + 'px';
        title.style.display = 'inline-block'; // Ensure the title is treated as a block element
        while (title.scrollWidth * 2 > parentWidth && fontSize > 12) {
            fontSize--;
            title.style.fontSize = fontSize + 'px';
        }
    });
});