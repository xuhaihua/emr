$(document).ready(function(){

    var currentPage = localStorage.getItem("currentPage")
    if(currentPage) {
        var list = $('.menu .menuItem');
        for(var i = 0; i < list.length; i++) {
            var item = list[i];

            if(item.getAttribute("href") == currentPage) {
                if(item.parentElement.parentElement.getAttribute("class").split(" ").indexOf("mainMenu") != -1) {
                    $(item.parentElement.parentElement).find(".mainMenuTitle").addClass("isExpand");
                    $(item.parentElement.parentElement).find(".submenu").slideDown(100);
                }

                $(item).addClass("isSelected");
                var frame = document.getElementById("frame");

                var currentChild = localStorage.getItem("currentChild");

                if(currentChild) {
                    frame.src = currentChild;
                } else {
                    frame.src = currentPage;
                }
            }
        }
    }


    $('.menu .mainMenuTitle').mousedown(function() {
        var clsList = $(this).attr("class").split(" ");
        if(clsList.indexOf("isExpand") == -1) {
            $(this).addClass("isExpand");
            $('ul',this.parentElement).slideDown(100);
        } else {
            $(this).removeClass("isExpand");
            $('ul',this.parentElement).slideUp(100);
        }
    });

    $('.menu .menuItem').mousedown(function() {

        if(this.getAttribute("href") == localStorage.getItem("currentPage") && !localStorage.getItem("currentChild")) {
            return;
        }

        var clsList = $(this).attr("class").split(" ");
        if(!localStorage.getItem("currentChild") || this.getAttribute("href") != localStorage.getItem("currentPage")) {
            $('.isSelected').removeClass("isSelected");
        }
        
        if(clsList.indexOf("isSelected") == -1 && this.getAttribute("href") != localStorage.getItem("currentPage")) {
            $(this).addClass("isSelected");
        }
        var frame = document.getElementById("frame");
        frame.src = this.getAttribute("href");

        localStorage.setItem("currentChild", "");

        localStorage.setItem("currentPage", this.getAttribute("href"));
    });
});


