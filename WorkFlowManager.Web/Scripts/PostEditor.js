$(document).ready(function () {
    var $tagEditor = $(".post-tag-editor");

    function PaydasSec($this) {
        var $tagParent = $this.closest("li");
        $tagParent.toggleClass("selected");

        var selected = $tagParent.hasClass("selected");
        $tagParent.find(".selected-input").val(selected);
    }

    $tagEditor
        .find(".tag-select")
        .on("click", "> li > a", function (e) {
            e.preventDefault();

            var $this = $(this);

            PaydasSec($this)

        });

    var $addTagButton = $tagEditor.find(".add-tag-button");
    var $newTagName = $tagEditor.find(".new-tag-name");

    $addTagButton.click(function (e) {
        e.preventDefault();
        addTag($newTagName.val());
    });

    $newTagName
        .keyup(function () {
            if ($newTagName.val().trim().length > 0)
                $addTagButton.prop("disabled", false);
            else
                $addTagButton.prop("disabled", true);
        })
        .keydown(function(e){
            if (e.which != 13)
                return;
            e.preventDefault();
            addTag($newTagName.val());
        });

    function addTag(name) {
        var newIndex = $tagEditor.find(".tag-select > li").size() - 1;
        
        $('input[name^="Paydaslar["][name$="].Name"][value="'+name+'"]').each(function () {//Bilgi daha önce girilmiş ise
            PaydasSec($(this));
        });
        
        if ($('input[name^="Paydaslar["][name$="].Name"][value="' + name + '"]').length == 0) //Değer bulunamazsa
        {
            //alert(name + 'Eklenmeli');
            $tagEditor
                .find(".tag-select > li.template")
                .clone()
                .removeClass("template")
                .addClass("selected")
                .find(".name").text(name).end()
                .find(".name-input").val(name).attr("name", "Paydaslar[" + newIndex + "].Name").end()
                .find(".selected-input").attr("name", "Paydaslar[" + newIndex + "].IsChecked").val(true).end()
                .appendTo($tagEditor.find(".tag-select"));

        }

        $newTagName.val("");
        $addTagButton.prop("disabled", true);
    }
});