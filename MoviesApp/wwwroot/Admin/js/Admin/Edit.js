// File upload preview
document.getElementById('fileInput').addEventListener('change', function (e) {
    const fileName = e.target.files[0].name;
    document.querySelector('.custom-file-upload label').textContent = fileName;