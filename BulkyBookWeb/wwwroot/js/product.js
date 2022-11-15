var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Amin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "20%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "category.name", "width": "10%" },
            { "data": "coverType.name", "width": "10%" },
            {
                "data": "id", //we need id for edit or delete
                "render": function (data) {
                    //html code
                    return `
                        <div class="col">
                            <a href="/Admin/Product/Upsert?id=${data}"
                           class="btn btn-outline-light btn-sm p-2"><i class="bi bi-pencil"></i>Edit</a>
                            <a onClick=Delete('/Admin/Product/Delete/${data}')
                           class="btn btn-outline-danger btn-sm p-2"><i class="bi bi-trash3"></i>Delete</a>
                        </div>
                    `
                },
                "width": "20%"
            }
        ]
    });
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