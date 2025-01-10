function getPDF() {

    var HTML_Width = $("#Invoice_Detail").width();
    var HTML_Height = $("#Invoice_Detail").height();

    var PDF_Width = HTML_Width;
    var PDF_Height = HTML_Height;

    var canvas_image_width = HTML_Width;
    var canvas_image_height = HTML_Height;

    var totalPDFPages = Math.ceil(HTML_Height / PDF_Height) - 1;

    html2canvas($("#Invoice_Detail")[0], { allowTaint: true }).then(function (canvas) {
        canvas.getContext('2d');
        //console.log(canvas.height + "  " + canvas.width);

        var imgData = canvas.toDataURL("image/jpeg", 1.0);
        var pdf = new jsPDF('p', 'pt', [PDF_Width, PDF_Height]);
        pdf.addImage(imgData, 'JPG', 80, 0, canvas_image_width, canvas_image_height);

        for (var i = 1; i <= totalPDFPages; i++) {
            pdf.addPage(PDF_Width, PDF_Height);
            pdf.addImage(imgData, 'JPG', 0, -(PDF_Height * i), canvas_image_width, canvas_image_height);
        }

        var url = pdf.output('datauristring');
        $('#PDFfile').prop("src", url);
        $("#modal_PDF").modal("show");

        //pdf.save("HTML-Document.pdf");
    });
};
