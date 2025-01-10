document.querySelector('.custom-select-wrapper').addEventListener('click', function () {
    this.querySelector('.custom-select').classList.toggle('open');
});


for (const option of document.querySelectorAll(".custom-option")) {
    option.addEventListener('click', function () {
        if (!this.classList.contains('selected')) {
            if (this.parentNode.querySelector('.custom-option.selected')) {
                this.parentNode.querySelector('.custom-option.selected').classList.remove('selected');
            }
            
            this.classList.add('selected');
            this.closest('.custom-select').querySelector('.custom-select__trigger span').textContent = this.textContent;
            
            //fillter
            //console.log(this.id);
            let value = this.id.split("|");//id: [tag_name]|tag_color
            let id = value[0];
            let color = value[1];

            let data_filter_arr = ["0", "0", "0"];
            if (window.location.toString().search(/development/ig) > -1) {
                data_filter_arr = JSON.parse(dev_filter_default);
            }
            else {
                data_filter_arr = JSON.parse(ticket_filter_default);
            }
            //console.log(window.location);

            if (id != "0") {
                table.columns([8]).search(id).draw();
                //arr luu default cac gia tri filter ticket
                data_filter_arr[2] = this.id;

                $("#-1").css("color", "#FFFFFF");
            }
            else {
                table.columns([8]).search('').draw();
                //arr luu default cac gia tri filter ticket
                data_filter_arr[2] = this.id;
                
                $("#-1").css("color", "black");
            }

            localStorage.setItem("dev_filter_default", JSON.stringify(data_filter_arr));
            $("#-1").css("background-color", color);
            $(".custom-select__trigger").css("line-height", "20px");
        }
    })
};

window.addEventListener('click', function (e) {
    const select = document.querySelector('.custom-select')
    if (!select.contains(e.target)) {
        select.classList.remove('open');
    }
});