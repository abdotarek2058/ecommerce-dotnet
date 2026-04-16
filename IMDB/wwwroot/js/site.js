document.addEventListener("DOMContentLoaded", function () {

    const skeleton = document.getElementById("skeleton-container");
    const movies = document.getElementById("movies-container");
    const pagination = document.getElementById("movies-pagination");
    const input = document.getElementById("searchInput");

    let timeout;
    let controller;

    // Skeleton
    setTimeout(function () {
        if (skeleton) skeleton.style.display = "none";
        if (movies) movies.style.display = "flex";
    }, 1000);

    if (!input || !movies) return;

    function fetchMovies(query) {

        if (controller) {
            controller.abort();
        }

        controller = new AbortController();

        movies.innerHTML = `
            <div class="col-12 d-flex justify-content-center align-items-center py-5">
                <div class="text-center">
                    <div class="spinner-border text-info"></div>
                    <p class="mt-3 mb-0">Loading...</p>
                </div>
            </div>`;

        if (pagination) {
            pagination.innerHTML = "";
        }

        fetch(`/Movies?search=${encodeURIComponent(query)}&page=1`, {
            signal: controller.signal
        })
            .then(res => res.text())
            .then(data => {
                let parser = new DOMParser();
                let doc = parser.parseFromString(data, "text/html");

                let newMovies = doc.querySelector("#movies-container")?.innerHTML;
                let newPagination = doc.querySelector("#movies-pagination")?.innerHTML;

                if (newMovies) {
                    movies.innerHTML = newMovies;
                } else {
                    movies.innerHTML = "<div class='col-12 text-center py-4'>No results found</div>";
                }

                if (pagination) {
                    pagination.innerHTML = newPagination ?? "";
                }

                const url = query ? `/Movies?search=${encodeURIComponent(query)}` : "/Movies";
                window.history.replaceState({}, "", url);
            })
            .catch(err => {
                if (err.name !== "AbortError") {
                    console.error(err);
                }
            });
    }

    input.addEventListener("input", function () {

        clearTimeout(timeout);

        let query = this.value.trim();

        timeout = setTimeout(() => {
            fetchMovies(query);
        }, 250);

    });

});
