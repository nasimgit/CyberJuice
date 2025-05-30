$(function () {
    var l = abp.localization.getResource('Cyberjuice');
    var createModal = new abp.ModalManager(abp.appPath + 'Departments/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'Departments/EditModal');

    var dataTable = $('#DepartmentsTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(cyberjuice.departments.department.getList),
            columnDefs: [
                {
                    title: l('Name'),
                    data: 'name'
                },
                {
                    title: l('Description'),
                    data: 'description'
                },
                {
                    title: l('EmployeeCount'),
                    data: 'employeeCount'
                },
                {
                    title: l('CreationTime'),
                    data: 'creationTime',
                    render: function (data) {
                        return luxon.DateTime.fromISO(data).toLocaleString(luxon.DateTime.DATETIME_SHORT);
                    }
                },
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('Cyberjuice.Departments.Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('Cyberjuice.Departments.Delete'),
                                confirmMessage: function (data) {
                                    return l('DepartmentDeletionConfirmationMessage', data.record.name);
                                },
                                action: function (data) {
                                    cyberjuice.departments.department
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.info(l('SuccessfullyDeleted'));
                                            dataTable.ajax.reload();
                                        });
                                }
                            }
                        ]
                    }
                }
            ]
        })
    );

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    $('#NewDepartmentButton').on('click', function (e) {
        e.preventDefault();
        createModal.open();
    });

}); 