async function fetchData() {
  const resultEl = document.getElementById('result');

  resultEl.classList.remove('hidden');
  resultEl.textContent = 'please stand by...';

  try {
    const response = await fetch('http://localhost:5269/api/Home/');

    if (!response.ok) {
      throw new Error(`SERVER FATAL: ${response.status}`);
    }

    const data = await response.json();
    resultEl.textContent = JSON.stringify(data, null, 2);

  } catch (error) {
    resultEl.textContent = `FATAL: ${error.message}`;
  }
}