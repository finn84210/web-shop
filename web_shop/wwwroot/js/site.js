const cartStorageKey = "matrixIncCart";
const cartCount = document.querySelector("#cart-count");
const selectedCount = document.querySelector("#selected-count");
const selectedTotal = document.querySelector("#selected-total");
const cartList = document.querySelector("#cart-list");
const emptyCart = document.querySelector("#empty-cart");
const euroFormatter = new Intl.NumberFormat("nl-NL", {
  style: "currency",
  currency: "EUR"
});

function readCart() {
  try {
    return JSON.parse(localStorage.getItem(cartStorageKey) || "[]");
  } catch {
    return [];
  }
}

function writeCart(productIds) {
  localStorage.setItem(cartStorageKey, JSON.stringify([...new Set(productIds)]));
  updateCartCount();
}

function updateCartCount() {
  if (!cartCount) {
    return;
  }

  cartCount.textContent = readCart().length.toString();
}

function addToCart(productId) {
  const cart = readCart();
  cart.push(Number(productId));
  writeCart(cart);
}

function removeFromCart(productId) {
  writeCart(readCart().filter((id) => id !== Number(productId)));
  renderCart();
}

function renderCart() {
  if (!cartList) {
    updateCartCount();
    return;
  }

  const products = JSON.parse(cartList.dataset.products || "[]");
  const cartIds = readCart();
  const cartProducts = products.filter((product) => cartIds.includes(product.Id));

  cartList.querySelectorAll(".cart-item, input[name='SelectedProductIds']").forEach((item) => item.remove());

  if (emptyCart) {
    emptyCart.hidden = cartProducts.length > 0;
  }

  cartProducts.forEach((product) => {
    const hiddenInput = document.createElement("input");
    hiddenInput.type = "hidden";
    hiddenInput.name = "SelectedProductIds";
    hiddenInput.value = product.Id;
    cartList.appendChild(hiddenInput);

    const item = document.createElement("article");
    item.className = "cart-item";
    item.innerHTML = `
      <div>
        <span class="category-pill">${product.Category}</span>
        <h2>${product.Name}</h2>
        <p>${product.Description}</p>
      </div>
      <strong>${euroFormatter.format(product.Price)}</strong>
      <button class="btn btn-outline-danger btn-sm remove-from-cart" type="button" data-product-id="${product.Id}">Verwijderen</button>
    `;
    cartList.appendChild(item);
  });

  if (selectedCount) {
    selectedCount.textContent = cartProducts.length.toString();
  }

  if (selectedTotal) {
    const total = cartProducts.reduce((sum, product) => sum + product.Price, 0);
    selectedTotal.textContent = euroFormatter.format(total);
  }

  updateCartCount();
}

document.querySelectorAll(".add-to-cart").forEach((button) => {
  button.addEventListener("click", () => {
    addToCart(button.dataset.productId);
    button.textContent = "Toegevoegd";
    window.setTimeout(() => {
      button.textContent = "In winkelmand";
    }, 1200);
  });
});

document.addEventListener("click", (event) => {
  const button = event.target.closest(".remove-from-cart");
  if (button) {
    removeFromCart(button.dataset.productId);
  }
});

document.querySelector(".cart-page")?.addEventListener("submit", () => {
  localStorage.removeItem(cartStorageKey);
});

renderCart();
updateCartCount();
