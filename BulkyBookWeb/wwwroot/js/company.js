﻿var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    console.log('dAD')
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Company/GetAll"
        },
        "columns": [
            { "data": "name", "width": "15%" },
            { "data": "streetAdrress", "width": "15%" },
            { "data": "city", "width": "15%" },
            { "data": "state", "width": "15%" },
            { "data": "phoneNumber", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <div class="col">
                            <a href="/Admin/Company/Upsert?id=${data}"
                           class="btn btn-outline-light btn-sm p-2"><i class="bi bi-pencil"></i>Edit</a>
                            <a onClick=Delete('/Admin/Company/Delete/${data}')
                           class="btn btn-outline-danger btn-sm p-2"><i class="bi bi-trash3"></i>Delete</a>
                        </div>
                    `
                },
                "width": "20%"
            }
        ]
    })
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    })
}