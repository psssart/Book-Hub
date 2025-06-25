// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var showFilterMenuButton;
var selectedAuthorsGuids = [];
var selectedGenresGuids = [];
var selectedPublishersGuids = [];
var selectedWarehousesGuids = [];
$(document).ready(function() {

    // Watching for publishers selection for filtering
    $('.dropdownPub-content input[type="checkbox"]').change(function() {
        let publisherId = $(this).val();

        if($(this).prop('checked')) {
            selectedPublishersGuids.push(publisherId);
        } else {
            let index = selectedPublishersGuids.indexOf(publisherId);
            if (index !== -1) {
                selectedPublishersGuids.splice(index, 1);
            }
        }
        sendSortingRequest($('#sortSelect').val(), getSortDirection())
    });

    // Watching for authors selection for filtering
    $('.dropdownAut-content input[type="checkbox"]').change(function() {
        let authorId = $(this).val();

        if($(this).prop('checked')) {
            // Add the GUID to the list
            selectedAuthorsGuids.push(authorId);
        } else {
            let index = selectedAuthorsGuids.indexOf(authorId);
            if (index !== -1) {
                selectedAuthorsGuids.splice(index, 1);
            }
        }
        sendSortingRequest($('#sortSelect').val(), getSortDirection())
    });

    // Watching for genres selection for filtering
    $('.dropdownGen-content input[type="checkbox"]').change(function() {
        let genreId = $(this).val();

        if($(this).prop('checked')) {
            selectedGenresGuids.push(genreId);
        } else {
            let index = selectedGenresGuids.indexOf(genreId);
            if (index !== -1) {
                selectedGenresGuids.splice(index, 1);
            }
        }
        sendSortingRequest($('#sortSelect').val(), getSortDirection())
    });

    // Show filtering menu
    $('.filterMenuButton').on('click', function() {
        // Toggle the visibility of the filter panel
        if ($('#filterPanel').css('right') === '0px') {
            //Save filter panel state
            showFilterMenuButton = false;
            // Hide panel
            $('#filterPanel').css('right', '-300px');
        } else {
            showFilterMenuButton = true;
            $('#filterPanel').css('right', '0px');
        }
    });

    // Reset filtering settings
    $('.filterResetbtn').on('click', function() {
        selectedAuthorsGuids = [];
        selectedGenresGuids = [];
        selectedPublishersGuids = [];
        selectedWarehousesGuids = [];
        sendSortingRequest($('#sortSelect').val(), getSortDirection(), false)
    });

    // Event listener for sorting selection change
    $('#sortSelect').change(function() {
        let sortBy = $(this).val();
        let sortDirection = getSortDirection();
        sendSortingRequest(sortBy, sortDirection);
    });

    function getSortDirection() {
        return $('.pyramid').hasClass('rotate') ? 'descending' : 'ascending';
    }

    // Function to send AJAX request for sorting
    function sendSortingRequest(sortBy, sortDirection, cloneCheckboxes=true) {
        $.ajax({
            url: '/Home/Index',
            method: 'GET',
            data: {
                searchInput: $('.search-form-txt-auth').val(),
                sortBy: sortBy,
                sortDirection: sortDirection,
                selectedAuthorsGuidsJson: JSON.stringify(selectedAuthorsGuids),
                selectedGenresGuidsJson: JSON.stringify(selectedGenresGuids),
                selectedPublishersGuidsJson: JSON.stringify(selectedPublishersGuids),
                selectedWarehousesGuidsJson: JSON.stringify(selectedWarehousesGuids)
            },
            success: function(response) {
                // DEBUGG
                console.log("Sorted|&Filtered");

                // Save filtering values
                let selectedAuthorsGuidsCopy = selectedAuthorsGuids;
                let selectedGenresGuidsCopy = selectedGenresGuids;
                let selectedPublishersGuidsCopy = selectedPublishersGuids;
                let selectedWarehousesGuidsCopy = selectedWarehousesGuids;

                if (cloneCheckboxes) {
                    // Save filtering menu authors checkboxes states
                    var $savedAuthorsCheckBoxes = $('.dropdownAut-content').clone();
                    var $savedGenresCheckBoxes = $('.dropdownGen-content').clone();
                    var $savedPublishersCheckBoxes = $('.dropdownPub-content').clone();
                    var $savedWarehousesCheckBoxes = $('.dropdownWar-content').clone();
                }

                // Update page with sorted books
                $('body').html(response);

                // Set sortBy and sortDirection after updating the page content
                $('#sortSelect').val(sortBy);
                if (sortDirection === 'descending') {
                    rotatePyramid();
                }

                // Show Filter Menu if it was on page until updating
                if (showFilterMenuButton) {
                    $('#filterPanel').css('right', '0px');
                }

                if (cloneCheckboxes) {
                    // Place filtering menu checkboxes states
                    $('.dropdownAut-content').replaceWith($savedAuthorsCheckBoxes);
                    $('.dropdownGen-content').replaceWith($savedGenresCheckBoxes);
                    $('.dropdownPub-content').replaceWith($savedPublishersCheckBoxes);
                    $('.dropdownWar-content').replaceWith($savedWarehousesCheckBoxes);
                }

                // Place filtering values back
                selectedAuthorsGuids = selectedAuthorsGuidsCopy;
                selectedGenresGuids = selectedGenresGuidsCopy;
                selectedPublishersGuids = selectedPublishersGuidsCopy;
                selectedWarehousesGuids = selectedWarehousesGuidsCopy;
            },
            error: function(xhr, status, error) {
                console.log("status: " + status + "\nXHR: " + xhr + "\nerror: " + error); // Log any errors
            }
        });
    }

    // Function to handle button click event
    $('#rotateButton').click(function() {
        rotatePyramid();
        // Retrieve sorting criteria and direction
        let sortBy = $('#sortSelect').val();
        let sortDirection = getSortDirection();
        sendSortingRequest(sortBy, sortDirection);
    });
});

// Function to toggle pyramid rotation
function rotatePyramid() {
    let pyramid = document.getElementById("rotateButton").querySelector(".pyramid");
    pyramid.classList.toggle("rotate");
}