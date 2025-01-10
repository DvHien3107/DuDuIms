/*
HTML REVIEW IMAGE

 <div style="padding-left:0px;display:block" class="col-sm-12" id="review_pic0">
    <p class="col-sm-12" style="border:1px dotted red; padding:3px">
        <img id="img_pic0" src="@picture" style="height:100px;margin-left:0" alt="xem trước" />
        <i style="padding-left:5px" id="fname_pic0"></i>
        <a onclick="upload_delete('pic0')" href="#" data-toggle="tooltip" class="pull-right" title="Xóa file này">
            <i class="fa fa-trash"></i>
        </a>
    </p>
    <input type="hidden" name="hdPicDelete_pic0" id="hdPicDelete_pic0" value="0" />
</div>

*/


    /**
 * Xem truoc image duoc chon
 * @param {any} input: doi tuong input file
 */
function reviewUpload(input, hide_after_show=false) {
    
        if (input.files && input.files[0]) {

            var inputid = $(input).attr("id");

            var reader = new FileReader();

            if (input.files[0].size > 10485760) {
                warning("File is too large limit size to less than 10 MB");
                $(input).closest(".upload_item").remove();
                return;
            }

            reader.onload = function (e) {
                var fname = $(input).val().split('\\').pop().toLowerCase();
                var strCheckImg = new RegExp(/\.(jpe?g|png|gif|bmp|svg)$/i);
                var strCheckPdf = new RegExp(/\.pdf$/i);
                var strCheckWord = new RegExp(/\.doc?x$/i);
                var strCheckExcel = new RegExp(/\.xls$/i);
                var strDeniedType = new RegExp(/(\.|\/)(bat|exe|cmd|sh|php([0-9])?|pl|cgi|386|dll|com|torrent|js|app|jar|pif|vb|vbscript|wsf|asp|cer|csr|jsp|drv|sys|ade|adp|bas|chm|cpl|crt|csh|fxp|hlp|hta|inf|ins|isp|jse|htaccess|htpasswd|ksh|lnk|mdb|mde|mdt|mdw|msc|msi|msp|mst|ops|pcd|prg|reg|scr|sct|shb|shs|url|vbe|vbs|wsc|wsf|wsh)$/i);
                var imgSrc = e.target.result;

             //   alert(imgSrc)
                if (strDeniedType.test(fname)) {
                    warning("This file type not allowed");
                    $(input).closest(".upload_item").remove();
                    return;
                }
                if (strCheckImg.test(fname) == false) {
                    //default image                             
                    //
                    if (window.FileIcons) {
                        //$("#img_" + inputid).hide();
                        imgSrc = "";
                        var icons = window.FileIcons;
                        var class_name = icons.getClassWithColor(fname);
                        $("#img_" + inputid).replaceWith($("<div style='font-size:30px;line-height:30px;height: 30px; color: var(--main-color-1)' class=" + class_name + "></div>"));
                    } else if (strCheckPdf.test(fname) == true) {
                        imgSrc = "/upload/img/pdf.png";
                    }
                    else if (strCheckWord.test(fname) == true) {
                        imgSrc = "/upload/img/word.png";
                    }
                    else if (strCheckExcel.test(fname) == true) {
                        imgSrc = "/upload/img/excel.png";
                    }
                    else {
                        imgSrc = "/upload/img/no_image.jpg";
                    }
                   
                }
               
                $("#img_" + inputid).attr("src", imgSrc);
                $("#fname_" + inputid).html(fname);
//                $("#hdPicDelete_" + inputid).val("0");
                $("#review_" + inputid).show();
            }

            reader.readAsDataURL(input.files[0]);
            if (hide_after_show) { $(input).hide() }
        }
        else {
            $("#img_" + inputid).attr("src", "");
            $("#fname_" + inputid).html("");
            $("#review_" + inputid).hide();
        }

    }



/**
 * huy bo file da chon
 * @param {any} inputid : id cua input file
 */
function upload_delete(inputid) {
    $("#" + inputid).val("");
    $("#img_" + inputid).attr("src", "");
    $("#hdPicDelete_" + inputid).val("1");
    $("#review_" + inputid).hide();
}

/**
 * xoa tap tin tu database d/v file duoc save vao uploadmorefile table
 * @param {any} id : uploadid
 */
function fdelete(id) {
    if (confirm("Are you sure want to delete?")) {
        $("#floading_" + id).show();
        var obj = {};
        obj.url = "/upload/morefiledeletebyid/";
        obj.type = "post";
        obj.datatype = "json";
        obj.contenttype = "application/json";
        obj.data = { "id": id };
        obj.success = function (data) {
            if (data == true) {
                $("#f_" + id).hide(500);
                var Success = $.parseJSON('{"text":"Delete successfully!", "layout":"topRight", "type":"success"}');
                noty(Success);
            }
        };
        obj.error = function () {
            var Error = $.parseJSON('{"text":"Error: can not delete file!", "layout":"topRight", "type":"success"}');
            noty(Error);
            $("#floading_" + id).hide();
        };
        obj.complete = function () { $("#floading_" + id).hide(); };
        jQuery.ajax(obj);
    }
    
}

/**
 * ten file co tien to, vd: 190825123025444_FILENAME.jpg
 * @param {any} id con so dau tien cua ten file.(id file)
 * @param {any} table_id id table
 * @param {any} table_name ten table
 * @param {any} column_name ten column chua file
 */
function fdelete_with_filename_id(id, table_id, table_name, column_name) {
    if (confirm("Are you sure want to delete?")) {
        $("#floading_" + id).show();
        var obj = {};
        obj.url = "/upload/filedelete/";
        obj.type = "post";
        obj.datatype = "json";
        obj.contenttype = "application/json";
        obj.data = {
            "id": id,
            table_id: table_id,
            table_name: table_name,
            column_name: column_name
        };
        obj.success = function (data) {
            if (data == true) {
                $("#f_" + id).hide(500);
                alert("Delete successfully");
            }
        };
        obj.error = function () { alert("Can not delete."); $("#floading_" + id).hide(); };
        obj.complete = function () { $("#floading_" + id).hide(); };
        jQuery.ajax(obj);
    }

}
