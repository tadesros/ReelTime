export function scrollByAmount(element, direction) {
    if (!element) return;
    const amount = element.clientWidth * 0.8 * direction;
    element.scrollBy({ left: amount, behavior: "smooth" });
}
