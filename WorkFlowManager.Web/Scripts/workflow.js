$(document).ready(function () {
    $("a[data-post]").click(function (e) {

        e.preventDefault();

        var $this = $(this);
        var message = $this.data("post");

        if (message && !confirm(message))
            return;

        var antiForgeryToken = $("#anti-forgery-form input");
        var antiForgeryInput = $("<input type='hidden'>").attr("name", antiForgeryToken.attr("name")).val(antiForgeryToken.val());
        $("<form>")
            .attr("method", "post")
            .attr("action", $this.attr("href"))
            .append(antiForgeryInput)
            .appendTo(document.body)
            .submit();
    });

    $("[data-confirm]").click(function (e) {
        var $this = $(this);
        var message = $this.data("confirm");

        if (message && !confirm(message)) {
            e.preventDefault();
        }
    });

});


function onDataBound(e) {
    var refreshButton = $(".k-grid-toolRefreshGrid");
    refreshButton.addClass("pull-right");
    refreshButton.attr('title','Yenile');
    refreshButton.find("span").addClass("fa fa-refresh");
}

function gridRefresh(gridId, modelId){
    $('#'+gridId).data('kendoGrid').dataSource.read(modelId);
}

function gridError(args) {

    $(".k-grid").each(function () {
        var grid = $(this).data("kendoGrid");
        if (grid !== null && grid.dataSource == args.sender) {
            if (args.errors) {
                grid.one("dataBinding", function (e) {
                    e.preventDefault();   // cancel grid rebind if error occurs

                    for (var error in args.errors) {
                        showMessage(grid.editable.element, error, args.errors[error].errors);
                    }
                });
            }
        }
    });
}

function showMessage(container, name, errors) {
    //add the validation message to the form
    container.find("[data-valmsg-for=" + name + "],[data-val-msg-for=" + name + "]")
    .replaceWith(validationMessageTmpl({ field: name, message: errors[0] }))
}


var $WF = {

    IslAkisiGoruntule: function (mermaidAPI, text, element) {
        mermaidAPI.render(element.id, text, function (svgCode) {
            element.innerHTML = svgCode;
            element.firstChild.style.height = element.getAttribute('viewBox').split(' ')[3] + 'px'
            element.firstChild.style.width = "5000px";
        });
    },

    MaskActivate: function () {
        $('.money').inputmask({ alias: "currency", prefix: '', digits: 2, radixPoint: ",", groupSeparator: "." });
        $('.date').inputmask({ alias: "date" });
    },

    AddExtensionClass: function (extension) {
        switch (extension) {
            case '.jpg':
            case '.img':
            case '.png':
            case '.gif':
                return "fa-file-image-o";
            case '.doc':
            case '.docx':
                return "fa-file-word-o";
            case '.xls':
            case '.xlsx':
                return "fa-file-excel-o";
            case '.pdf':
                return "fa-file-pdf-o";
            case '.zip':
            case '.rar':
                return "fa-file-archive-o";
            default:
                return "fa-file-o";
        }
    },

    ModalOpenClick: function (element, e, message) {
        e.preventDefault();
        if (message != '') {
            waitingDialog.show(message);
        } else {
            waitingDialog.show();
        }
        var $this = element;
        var remote = $this.data('load-remote');
        if (remote) {
            var title = $this.data('title');
            if (title) {
                $('.modal-title').text(title);
            }
            $($this.data('remote-target')).load(remote);
        }
    },

    GetCookieParameter: function (parameter) {
        var cookiename = $('#userEmail').text() + "_f" + parameter;

        var paramValue = $.cookie(cookiename);
        if (paramValue == undefined) {
            paramValue = 0;
        }
        return paramValue;
    },


    FaaliyetListesi: function (link) {

        var projeId = $WF.GetCookieParameter("proje");
        var birimId = $WF.GetCookieParameter("birim");

        link = link + '?page=' + $('#page').val();

        if (projeId != 0) {
            link = link + '&projeId=' + projeId;
        } else {
            link = link + '&birimId=' + birimId;
        }
        document.location.href = link;
    },

    SetBirimProje: function () {
        var birimId = $WF.GetCookieParameter("birim");
        var projeId = $WF.GetCookieParameter("proje");
        console.log('proje id :' + projeId);
        console.log('birim id :' + birimId);
        $('#birimId').val(birimId);
        $('#projeId').val(projeId);
    }

}
