let src = '';
let modal = null;

function createEmbed() {
    document.getElementById("modal-trailer").setAttribute("src", src);
}

function destroyEmbed() {
    document.getElementById("modal-trailer").setAttribute("src", "");
}

export function initVideoPlayer(videoUrl) {
    if (modal) {
        modal.removeEventListener('shown.bs.modal', createEmbed);
        modal.removeEventListener('hidden.bs.modal', destroyEmbed);
    }

    modal = document.getElementById('movie-modal');
    if (!modal) return;
    src = videoUrl;

    modal.addEventListener('shown.bs.modal', createEmbed);
    modal.addEventListener('hidden.bs.modal', destroyEmbed);
}