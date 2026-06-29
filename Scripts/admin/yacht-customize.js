(function () {
    // Read view-specific values emitted by Razor before this shared script loads.
    var config = window.yachtCustomizeConfig || {};

    // Dimension editor images are normalized to the legacy Tayana dimensions layout size.
    var DIMENSION_IMAGE_WIDTH = 278;
    var DIMENSION_IMAGE_HEIGHT = 345;

    function formatDefaultSummernoteImage($image) {
        // Overview images should scale with Bootstrap content width.
        $image.addClass("img-fluid");
    }

    function formatDimensionSummernoteImage($image) {
        // Dimension images live inside a fixed-size table cell, so remove responsive
        // sizing and force a predictable visual box.
        var $cell = $image.closest(".dimension-image-cell");

        $image
            .removeClass("img-fluid")
            .addClass("dimension-fixed-image")
            .attr({
                width: DIMENSION_IMAGE_WIDTH,
                height: DIMENSION_IMAGE_HEIGHT
            })
            .css({
                width: DIMENSION_IMAGE_WIDTH + "px",
                height: DIMENSION_IMAGE_HEIGHT + "px",
                "object-fit": "contain",
                "object-position": "center center",
                "max-width": "none",
                display: "block",
                margin: "0 auto"
            });

        if ($cell.length) {
            // Keep only one image in the placeholder cell and remove helper text after upload.
            $cell.find("p").remove();
            $cell.find("img").not($image).remove();

            $cell
                .append($image)
                .css({
                    width: DIMENSION_IMAGE_WIDTH + "px",
                    height: DIMENSION_IMAGE_HEIGHT + "px",
                    "text-align": "center",
                    "vertical-align": "middle"
                });
        }
    }

    function uploadSummernoteImage(file, $editor, formatImage) {
        // Upload Summernote images immediately so the editor can insert a real URL into
        // the HTML content before the full Yacht form is submitted.
        var formData = new FormData();

        formData.append("file", file);

        // Include the MVC anti-forgery token because the upload action is a POST endpoint.
        var token = $('input[name="__RequestVerificationToken"]').val();

        if (token) {
            formData.append("__RequestVerificationToken", token);
        }

        if (!config.uploadEditorImageUrl) {
            // The view must provide the URL because this static file cannot call Url.Action.
            alert("Image upload URL is missing.");
            return;
        }

        $.ajax({
            url: config.uploadEditorImageUrl,
            type: "POST",
            data: formData,
            processData: false,
            contentType: false,

            success: function (response) {
                if (response && response.url) {
                    // Insert the uploaded image URL into the active editor, then apply the
                    // editor-specific formatting callback when one is supplied.
                    $editor.summernote("insertImage", response.url, function ($image) {
                        if (typeof formatImage === "function") {
                            formatImage($image);
                        }
                    });
                } else {
                    alert("Image upload failed. No image URL returned.");
                }
            },

            error: function (xhr) {
                var response = xhr.responseJSON || {};
                alert(response.error || "Image upload failed.");
            }
        });
    }

    function createUploadRow(inputName, accept) {
        // Build the same upload row shape used by the Razor markup for dynamically added files.
        var row = document.createElement("div");
        row.className = "upload-row mb-2";

        var input = document.createElement("input");
        input.type = "file";
        input.name = inputName;
        input.className = "form-control";

        if (accept) {
            // Image upload groups pass image/*; download files leave this unrestricted.
            input.accept = accept;
        }

        var removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-danger btn-sm mt-1";
        removeButton.innerText = "Remove";

        removeButton.addEventListener("click", function () {
            // Removing a row before submit prevents that empty/new input from being posted.
            row.parentNode.removeChild(row);
        });

        row.appendChild(input);
        row.appendChild(removeButton);

        return row;
    }

    function bindAddUploadButton(buttonId, listId, inputName, accept) {
        // Bind an Add File button to its upload list. Missing elements are ignored so
        // the same script can run on both Create and Edit pages.
        var button = document.getElementById(buttonId);
        var list = document.getElementById(listId);

        if (!button || !list) {
            return;
        }

        button.addEventListener("click", function () {
            // Append another input using the field name expected by YachtVM model binding.
            list.appendChild(createUploadRow(inputName, accept));
        });
    }

    function escapeHtml(text) {
        // Convert prompt text to safe HTML before inserting it into Summernote content.
        return $("<div>").text(text).html();
    }

    function DimensionTemplateButton(context) {
        // Custom Summernote button for building the dimensions table used by yacht pages.
        var ui = $.summernote.ui;

        return ui.button({
            contents: '<i class="note-icon-table"></i> Dimension Layout',
            tooltip: "Insert Dimension Layout",

            click: function () {
                // Prompt-driven template keeps the admin from hand-writing the repeated
                // table structure used by the public dimensions section.
                var title = prompt("Enter section title:", "46 DIMENSIONS");

                if (title === null) {
                    return;
                }

                title = title.trim();

                if (title === "") {
                    title = "DIMENSIONS";
                }

                var rowCountText = prompt("How many specification rows do you want?", "5");

                if (rowCountText === null) {
                    return;
                }

                var rowCount = parseInt(rowCountText, 10);

                if (isNaN(rowCount) || rowCount <= 0) {
                    alert("Please enter a valid row number.");
                    return;
                }

                var rowsHtml = "";

                for (var i = 1; i <= rowCount; i++) {
                    // Each row becomes one label/value pair in the nested spec table.
                    var label = prompt("Row " + i + " label:", "");
                    var value = prompt("Row " + i + " value:", "");

                    if (label === null || value === null) {
                        return;
                    }

                    rowsHtml +=
                        "<tr>" +
                        "<th>" + escapeHtml(label.trim()) + "</th>" +
                        "<td>" + escapeHtml(value.trim()) + "</td>" +
                        "</tr>";
                }

                var includeImage = confirm("Do you want to include an image area?");
                var imageCellHtml = "";

                if (includeImage) {
                    // The placeholder class is used later to size the uploaded image.
                    imageCellHtml =
                        '<td class="dimension-image-cell">' +
                        "<p>Click here, then use the picture button to upload image</p>" +
                        "</td>";
                }

                var html =
                    '<div class="box3">' +
                    "<h4>" + escapeHtml(title) + "</h4>" +
                    '<table class="table02">' +
                    "<tbody>" +
                    "<tr>" +
                    '<td class="table02td01">' +
                    "<table><tbody>" +
                    rowsHtml +
                    "</tbody></table>" +
                    "</td>" +
                    imageCellHtml +
                    "</tr>" +
                    "</tbody>" +
                    "</table>" +
                    "</div>";

                context.invoke("editor.pasteHTML", html);
            }
        }).render();
    }

    function insertSpecSection(context) {
        // Custom Summernote button for adding a titled bullet-list section to Specification.
        var ui = $.summernote.ui;

        return ui.button({
            contents: '<i class="note-icon-plus"></i> Spec Section',
            tooltip: "Insert specification section",

            click: function () {
                var sectionTitle = prompt("Enter section title, for example: HULL");

                if (!sectionTitle || !sectionTitle.trim()) {
                    return;
                }

                var itemsText = prompt(
                    "Enter items, one item per line:\n\n" +
                    "Hand laid up FRP hull\n" +
                    "Teak rubrail\n" +
                    "Engine cooling water intake strainer"
                );

                if (!itemsText || !itemsText.trim()) {
                    return;
                }

                context.invoke("editor.pasteHTML", buildSpecSectionHtml(sectionTitle, itemsText));
            }
        }).render();
    }

    function buildSpecSectionHtml(sectionTitle, itemsText) {
        // Convert one-item-per-line prompt input into safe list markup.
        var safeTitle = escapeHtml(sectionTitle.trim());

        var itemsHtml = itemsText
            .split(/\r?\n/)
            .map(function (item) {
                return item.trim();
            })
            .filter(function (item) {
                return item.length > 0;
            })
            .map(function (item) {
                return "<li>" + escapeHtml(item) + "</li>";
            })
            .join("");

        return "<p>" + safeTitle + "</p><ul>" + itemsHtml + "</ul>";
    }

    $(function () {
        // Wire up dynamic upload rows for the three multipart upload collections on YachtVM.
        bindAddUploadButton("add-deck-upload", "deck-upload-list", "DeckImgsUploads", "image/*");
        bindAddUploadButton("add-interior-upload", "interior-upload-list", "InteriorUploads", "image/*");
        bindAddUploadButton("add-downloadfile-upload", "downloadfile-upload-list", "DownloadFileUploads", null);

        // Cache the three rich-text fields because each one gets a different toolbar/profile.
        var $overview = $("#Overview");
        var $dimensions = $("#Dimensions");
        var $specification = $("#Specification");

        // Overview supports general rich text plus responsive inline images.
        $overview.summernote({
            height: 300,
            tooltip: false,
            placeholder: "Enter overview...",

            toolbar: [
                ["style", ["bold", "italic", "underline", "clear"]],
                ["para", ["ul", "ol"]],
                ["insert", ["picture", "link"]],
                ["view", ["codeview"]]
            ],

            callbacks: {
                onImageUpload: function (files) {
                    if (files && files.length > 0) {
                        uploadSummernoteImage(files[0], $overview, formatDefaultSummernoteImage);
                    }
                },

                onPaste: function (e) {
                    // Paste plain text only to avoid importing external HTML/CSS into page content.
                    e.preventDefault();

                    var clipboardData = e.originalEvent.clipboardData || window.clipboardData;
                    var text = clipboardData.getData("text/plain");

                    document.execCommand("insertText", false, text);
                }
            }
        });

        // Dimensions uses a constrained editor profile because the public layout expects
        // a specific table/image structure.
        $dimensions.summernote({
            height: 420,
            minHeight: 300,

            toolbar: [
                ["style", ["bold", "italic", "clear"]],
                ["para", ["paragraph"]],
                ["insert", ["picture", "table", "dimensionTemplate"]],
                ["view", ["fullscreen","codeview"]]
            ],

            buttons: {
                // Register the custom dimensions template button in the toolbar.
                dimensionTemplate: DimensionTemplateButton
            },

            popover: {
                image: [
                    ["float", ["floatLeft", "floatRight", "floatNone"]],
                    ["remove", ["removeMedia"]]
                ],
                table: [
                    ["add", ["addRowDown", "addRowUp"]],
                    ["delete", ["deleteRow", "deleteTable"]]
                ]
            },

            // Prevent dropped files from bypassing the upload handler and formatting rules.
            disableDragAndDrop: true,

            callbacks: {
                onImageUpload: function (files) {
                    if (files && files.length > 0) {
                        uploadSummernoteImage(files[0], $dimensions, formatDimensionSummernoteImage);
                    }
                }
            }
        });

        // Specification uses a custom helper to create repeated titled list sections.
        $specification.summernote({
            height: 600,
            dialogsInBody: true,

            toolbar: [
                ["custom", ["insertSpecSection"]],
                ["style", ["bold", "italic", "underline", "clear"]],
                ["para", ["ul", "ol", "paragraph"]],
                ["insert", ["link"]],
                ["view", ["codeview"]]
            ],

            buttons: {
                // Register the custom specification-section button in the toolbar.
                insertSpecSection: insertSpecSection
            }
        });
    });
})();
