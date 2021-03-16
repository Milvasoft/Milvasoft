(function () {
    window.addEventListener("load", function () {
        setTimeout(function () {

            var headContent = document.getElementsByTagName('head')[0];
            headContent.children[1].textContent = "Milvasoft.Helpers Sample API";
            headContent.children[3].href = "https://opsiyon.online/api/docs/favicon-32x32.png";
            headContent.children[4].href = "https://opsiyon.online/api/docs/favicon-16x16.png";


            var title = document.createElement("span");
            title.innerHTML = "Milvasoft";
            var logo = document.getElementsByClassName('link'); //For Changing The Link On The Logo Image
            logo[0].href = "https://www.milvasoft.com";
            logo[0].target = "_blank";
            logo[0].appendChild(title);
            logo[0].children[0].alt = "Milvasoft";
            logo[0].children[0].src = "https://opsiyon.online/api/docs/favicon.ico"; //For Changing The Logo Image

            var selectSpan = document.getElementsByClassName('select-label');
            selectSpan[0].children[0].innerHTML = "Version :";
            selectSpan[0].children[0].style.whiteSpace = "nowrap";

         
        });
    });
})();