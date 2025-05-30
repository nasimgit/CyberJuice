$(function () {
    var l = abp.localization.getResource('Cyberjuice');
    var createModal = new abp.ModalManager(abp.appPath + 'Companies/CreateModal');
    var editModal = new abp.ModalManager(abp.appPath + 'Companies/EditModal');

    var dataTable = $('#CompaniesTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            searching: true,
            scrollX: true,
            ajax: abp.libs.datatables.createAjax(cyberjuice.companies.company.getAllPaged),
            columnDefs: [
                {
                    title: l('Name'),
                    data: 'name'
                },
                {
                    title: l('Actions'),
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                action: function (data) {
                                    editModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: l('Delete'),
                                confirmMessage: function (data) {
                                    return l('WorkspaceDeletionConfirmationMessage', data.record.name);
                                },
                                action: function (data) {
                                    cyberjuice.companies.company
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

    $('#NewWorkspaceButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

}); 