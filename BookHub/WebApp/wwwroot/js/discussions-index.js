// State management
var showFilterMenuButton = false;
var selectedAuthorsGuids = [];
var selectedGenresGuids = [];
var selectedBooksGuids = [];
var searchTimeout = null;

$(document).ready(function() {

    // Dropdown toggle logic (reuse from Home/Index)
    $(document).on('click', '.dropdown-filter > .dropbtn', function(e) {
        e.stopPropagation();
        const $dropdown = $(this).closest('.dropdown-filter');
        $('.dropdown-filter').not($dropdown).removeClass('open');
        $dropdown.toggleClass('open');
    });

    $(document).on('click', '.dropdown-filter-content', function(e) {
        e.stopPropagation();
    });

    $(document).on('click', function() {
        $('.dropdown-filter').removeClass('open');
    });

    // Books filter
    $(document).on('change', '.dropdownBook-content input[type="checkbox"]', function(){
        let bookId = $(this).val();
        if($(this).prop('checked')) {
            selectedBooksGuids.push(bookId);
        } else {
            let index = selectedBooksGuids.indexOf(bookId);
            if (index !== -1) {
                selectedBooksGuids.splice(index, 1);
            }
        }
        sendFilterRequest();
    });

    // Authors filter
    $(document).on('change', '.dropdownAut-content input[type="checkbox"]', function() {
        let authorId = $(this).val();
        if($(this).prop('checked')) {
            selectedAuthorsGuids.push(authorId);
        } else {
            let index = selectedAuthorsGuids.indexOf(authorId);
            if (index !== -1) {
                selectedAuthorsGuids.splice(index, 1);
            }
        }
        sendFilterRequest();
    });

    // Genres filter
    $(document).on('change', '.dropdownGen-content input[type="checkbox"]', function() {
        let genreId = $(this).val();
        if($(this).prop('checked')) {
            selectedGenresGuids.push(genreId);
        } else {
            let index = selectedGenresGuids.indexOf(genreId);
            if (index !== -1) {
                selectedGenresGuids.splice(index, 1);
            }
        }
        sendFilterRequest();
    });

    // Filter panel show/hide
    $(document).on('click', '.filterMenuButton', function (e) {
        e.stopPropagation();
        const $panel = $('#filterPanel');
        const isOpen = $panel.css('right') === '0px';
        showFilterMenuButton = !isOpen;
        $panel.css('right', isOpen ? '-300px' : '0px');
    });

    $(document).on('click', '#filterPanel', function (e) {
        e.stopPropagation();
    });

    $(document).on('click', function (e) {
        const $t = $(e.target);
        if (!$t.closest('#filterPanel, .filterMenuButton').length) {
            $('#filterPanel').css('right', '-300px');
            showFilterMenuButton = false;
        }
    });

    $(document).on('click', '#hidePanelBtn', function (e) {
        $('#filterPanel').css('right', '-300px');
        showFilterMenuButton = false;
    });

    // Reset filters
    $(document).on('click', '.filterResetbtn', function() {
        selectedAuthorsGuids = [];
        selectedGenresGuids = [];
        selectedBooksGuids = [];

        // Uncheck all checkboxes
        $('.dropdown-filter-content input[type="checkbox"]').prop('checked', false);

        // Clear search
        $('#searchInput').val('');

        sendFilterRequest();
    });

    // Search input with debounce
    $(document).on('input', '#searchInput', function() {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(function() {
            sendFilterRequest();
        }, 500); // 500ms debounce
    });

    // Sort select change
    $(document).on('change', '#sortSelect', function() {
        sendFilterRequest();
    });

    // Sort direction toggle
    $(document).on('click', '#rotateButton', function() {
        rotatePyramid();
        sendFilterRequest();
    });

    // Function to send AJAX request
    function sendFilterRequest() {
        const searchInput = $('#searchInput').val();
        const sortBy = $('#sortSelect').val();
        const sortDirection = getSortDirection();

        $.ajax({
            url: '/Discussions/Index',
            method: 'GET',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            data: {
                searchInput: searchInput,
                sortBy: sortBy,
                sortDirection: sortDirection,
                selectedAuthorsGuidsJson: JSON.stringify(selectedAuthorsGuids),
                selectedGenresGuidsJson: JSON.stringify(selectedGenresGuids),
                selectedBooksGuidsJson: JSON.stringify(selectedBooksGuids)
            },
            success: function (html) {
                const $new = $(html);
                $('#results-root').replaceWith($new);

                // Restore UI state
                $('#sortSelect').val(sortBy);
                if (sortDirection === 'descending') {
                    $('#rotateButton .pyramid').addClass('rotate');
                } else {
                    $('#rotateButton .pyramid').removeClass('rotate');
                }

                if (showFilterMenuButton) {
                    $('#filterPanel').css('right', '0px');
                }

                // Restore checkbox states
                restoreCheckboxStates();
            },
            error: function (xhr, status, error) {
                console.error("Filter request failed:", status, error);
            }
        });
    }

    function getSortDirection() {
        return $('.pyramid').hasClass('rotate') ? 'descending' : 'ascending';
    }

    function restoreCheckboxStates() {
        $('.dropdownAut-content input[type=checkbox]').each(function() {
            $(this).prop('checked', selectedAuthorsGuids.includes($(this).val()));
        });
        $('.dropdownGen-content input[type=checkbox]').each(function() {
            $(this).prop('checked', selectedGenresGuids.includes($(this).val()));
        });
        $('.dropdownBook-content input[type=checkbox]').each(function() {
            $(this).prop('checked', selectedBooksGuids.includes($(this).val()));
        });
    }
});

// Pyramid rotation for sort direction
function rotatePyramid() {
    let pyramid = document.getElementById("rotateButton").querySelector(".pyramid");
    pyramid.classList.toggle("rotate");
}
