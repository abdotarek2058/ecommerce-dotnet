document.addEventListener("DOMContentLoaded", function () {

    const skeleton = document.getElementById("skeleton-container");
    const movies = document.getElementById("movies-container");
    const input = document.getElementById("searchInput");

    let timeout;
    let controller;

    // Skeleton
    setTimeout(function () {
        if (skeleton) skeleton.style.display = "none";
        if (movies) movies.style.display = "flex";
    }, 1000);

    if (!input) return;

    input.addEventListener("input", function () {

        clearTimeout(timeout);

        let query = this.value.trim();

        // 🔥 لو فاضي يرجع كل الداتا
        if (query === "") {
            location.reload();
            return;
        }

        timeout = setTimeout(() => {

            if (controller) {
                controller.abort();
            }

            controller = new AbortController();

            // Loading UI
            movies.innerHTML = `
                <div style="color:white; text-align:center; width:100%">
                    <div class="spinner-border text-light"></div>
                    <p>Loading...</p>
                </div>`;

            fetch(`/Movies?search=${query}`, {
                signal: controller.signal
            })
                .then(res => res.text())
                .then(data => {
                    let parser = new DOMParser();
                    let doc = parser.parseFromString(data, "text/html");

                    let newMovies = doc.querySelector("#movies-container")?.innerHTML;

                    if (newMovies) {
                        movies.innerHTML = newMovies;
                    } else {
                        movies.innerHTML = "<p style='color:white'>No results found</p>";
                    }
                })
                .catch(err => {
                    if (err.name !== "AbortError") {
                        console.error(err);
                    }
                });

        }, 500);

    });

});