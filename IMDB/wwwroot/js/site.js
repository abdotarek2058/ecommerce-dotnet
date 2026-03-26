window.addEventListener("load", function () {

    const skeleton = document.getElementById("skeleton-container");
    const movies = document.getElementById("movies-container");

    setTimeout(function () {

        skeleton.style.display = "none";
        movies.style.display = "flex";

    }, 2000);

});