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
    const rawCart = JSON.parse(localStorage.getItem(cartStorageKey) || "[]");

    if (!Array.isArray(rawCart)) {
      return [];
    }

    // Oude winkelwagens bevatten alleen product-id's. Die zetten we om naar items met aantallen.
    if (rawCart.every((item) => typeof item === "number")) {
      return rawCart.map((productId) => ({ productId: Number(productId), quantity: 1 }));
    }

    return rawCart
      .map((item) => ({
        productId: Number(item.productId),
        quantity: Math.max(1, Number(item.quantity) || 1)
      }))
      .filter((item) => item.productId > 0);
  } catch {
    return [];
  }
}

function writeCart(cartItems) {
  localStorage.setItem(cartStorageKey, JSON.stringify(cartItems));
  updateCartCount();
}

function updateCartCount() {
  if (!cartCount) {
    return;
  }

  const count = readCart().reduce((total, item) => total + item.quantity, 0);
  cartCount.textContent = count.toString();
}

function addToCart(productId) {
  const cart = readCart();
  const existingItem = cart.find((item) => item.productId === Number(productId));

  if (existingItem) {
    existingItem.quantity += 1;
  } else {
    cart.push({ productId: Number(productId), quantity: 1 });
  }

  writeCart(cart);
}

function removeFromCart(productId) {
  writeCart(readCart().filter((item) => item.productId !== Number(productId)));
  renderCart();
}

function changeQuantity(productId, quantity) {
  const cart = readCart();
  const item = cart.find((cartItem) => cartItem.productId === Number(productId));

  if (!item) {
    return;
  }

  item.quantity = Math.max(1, Number(quantity) || 1);
  writeCart(cart);
  renderCart();
}

function renderCart() {
  if (!cartList) {
    updateCartCount();
    return;
  }

  const products = JSON.parse(cartList.dataset.products || "[]");
  const cartItems = readCart();
  const cartProducts = cartItems
    .map((cartItem) => {
      const product = products.find((availableProduct) => availableProduct.Id === cartItem.productId);
      return product ? { ...product, Quantity: Math.min(cartItem.quantity, product.Stock) } : null;
    })
    .filter(Boolean);

  cartList.querySelectorAll(".cart-item, input.cart-input").forEach((item) => item.remove());

  if (emptyCart) {
    emptyCart.hidden = cartProducts.length > 0;
  }

  cartProducts.forEach((product, index) => {
    const productInput = document.createElement("input");
    productInput.type = "hidden";
    productInput.className = "cart-input";
    productInput.name = `CartItems[${index}].ProductId`;
    productInput.value = product.Id;
    cartList.appendChild(productInput);

    const quantityInput = document.createElement("input");
    quantityInput.type = "hidden";
    quantityInput.className = "cart-input";
    quantityInput.name = `CartItems[${index}].Quantity`;
    quantityInput.value = product.Quantity;
    cartList.appendChild(quantityInput);

    const item = document.createElement("article");
    item.className = "cart-item";
    item.innerHTML = `
      <div>
        <span class="category-pill">${product.Category}</span>
        <h2>${product.Name}</h2>
        <p>${product.Description}</p>
      </div>
      <div class="cart-quantity-control" aria-label="Aantal voor ${product.Name}">
        <button class="btn btn-outline-dark btn-sm cart-quantity-step" type="button" data-product-id="${product.Id}" data-step="-1">-</button>
        <input class="form-control cart-quantity-input" type="number" min="1" max="${product.Stock}" value="${product.Quantity}" data-product-id="${product.Id}" />
        <button class="btn btn-outline-dark btn-sm cart-quantity-step" type="button" data-product-id="${product.Id}" data-step="1">+</button>
      </div>
      <strong>${euroFormatter.format(product.Price * product.Quantity)}</strong>
      <button class="btn btn-outline-danger btn-sm remove-from-cart" type="button" data-product-id="${product.Id}">Verwijderen</button>
    `;
    cartList.appendChild(item);
  });

  if (selectedCount) {
    selectedCount.textContent = cartProducts.reduce((sum, product) => sum + product.Quantity, 0).toString();
  }

  if (selectedTotal) {
    const total = cartProducts.reduce((sum, product) => sum + product.Price * product.Quantity, 0);
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

  const quantityStepButton = event.target.closest(".cart-quantity-step");
  if (quantityStepButton) {
    const productId = quantityStepButton.dataset.productId;
    const step = Number(quantityStepButton.dataset.step);
    const input = document.querySelector(`.cart-quantity-input[data-product-id="${productId}"]`);
    const nextQuantity = Number(input?.value || 1) + step;
    changeQuantity(productId, nextQuantity);
  }
});

document.addEventListener("change", (event) => {
  const input = event.target.closest(".cart-quantity-input");
  if (!input) {
    return;
  }

  const max = Number(input.max || 999);
  const nextQuantity = Math.min(max, Math.max(1, Number(input.value) || 1));
  changeQuantity(input.dataset.productId, nextQuantity);
});

document.querySelector(".cart-page")?.addEventListener("submit", () => {
  localStorage.removeItem(cartStorageKey);
});

renderCart();
updateCartCount();
