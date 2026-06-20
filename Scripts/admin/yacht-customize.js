(function () {
    var config = window.yachtCustomizeConfig || {};

    function uploadSummernoteImage(file, $editor) {
        var formData = new FormData();

        formData.append("file", file);

        var token = $('input[name="__RequestVerificationToken"]').val();

        if (token) {
            formData.append("__RequestVerificationToken", token);
        }

        if (!config.uploadEditorImageUrl) {
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
                    $editor.summernote("insertImage", response.url, function ($image) {
                        $image.addClass("img-fluid");
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
        var row = document.createElement("div");
        row.className = "upload-row mb-2";

        var input = document.createElement("input");
        input.type = "file";
        input.name = inputName;
        input.className = "form-control";

        if (accept) {
            input.accept = accept;
        }

        var removeButton = document.createElement("button");
        removeButton.type = "button";
        removeButton.className = "btn btn-danger btn-sm mt-1";
        removeButton.innerText = "Remove";

        removeButton.addEventListener("click", function () {
            row.parentNode.removeChild(row);
        });

        row.appendChild(input);
        row.appendChild(removeButton);

        return row;
    }

    function bindAddUploadButton(buttonId, listId, inputName, accept) {
        var button = document.getElementById(buttonId);
        var list = document.getElementById(listId);

        if (!button || !list) {
            return;
        }

        button.addEventListener("click", function () {
            list.appendChild(createUploadRow(inputName, accept));
        });
    }

    function escapeHtml(text) {
        return $("<div>").text(text).html();
    }

    function DimensionTemplateButton(context) {
        var ui = $.summernote.ui;

        return ui.button({
            contents: '<i class="note-icon-table"></i> Dimension Layout',
            tooltip: "Insert Dimension Layout",

            click: function () {
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
        bindAddUploadButton("add-deck-upload", "deck-upload-list", "DeckImgsUploads", "image/*");
        bindAddUploadButton("add-interior-upload", "interior-upload-list", "InteriorUploads", "image/*");
        bindAddUploadButton("add-downloadfile-upload", "downloadfile-upload-list", "DownloadFileUploads", null);

        var $overview = $("#Overview");
        var $dimensions = $("#Dimensions");
        var $specification = $("#Specification");

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
                        uploadSummernoteImage(files[0], $overview);
                    }
                },

                onPaste: function (e) {
                    e.preventDefault();

                    var clipboardData = e.originalEvent.clipboardData || window.clipboardData;
                    var text = clipboardData.getData("text/plain");

                    document.execCommand("insertText", false, text);
                }
            }
        });

        $dimensions.summernote({
            height: 420,
            minHeight: 300,

            toolbar: [
                ["style", ["bold", "italic", "clear"]],
                ["para", ["paragraph"]],
                ["insert", ["picture", "table", "dimensionTemplate"]],
                ["view", ["fullscreen"]]
            ],

            buttons: {
                dimensionTemplate: DimensionTemplateButton
            },

            popover: {
                image: [
                    ["image", ["resizeFull", "resizeHalf", "resizeQuarter", "resizeNone"]],
                    ["float", ["floatLeft", "floatRight", "floatNone"]],
                    ["remove", ["removeMedia"]]
                ],
                table: [
                    ["add", ["addRowDown", "addRowUp"]],
                    ["delete", ["deleteRow", "deleteTable"]]
                ]
            },

            disableDragAndDrop: true,

            callbacks: {
                onImageUpload: function (files) {
                    if (files && files.length > 0) {
                        uploadSummernoteImage(files[0], $dimensions);
                    }
                }
            }
        });

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
                insertSpecSection: insertSpecSection
            }
        });
    });
})();