var SITE = (function ($) {
    var siteObject = {};
    siteObject.initialize = function () {

        $(document).ajaxStart(function () {
            $('#ajax_loader').show();

        }).ajaxStop(function () {
            $('#ajax_loader').hide();
        }).ajaxError(function (xhr, status) {
            $("#ajax-debug").text(status.responseText);
            alert("There is an ajax error, please view ajax-debug content for further information");
        });

        siteObject.search();

        $('#fromDate').on('change', function () {
            siteObject.search();
        });

        $('#toDate').on('change', function () {
            siteObject.search();
        });

        $('#leagues').on('change', function () {
            siteObject.search();
        });

        $('#auto-refresh').on('change', function () {
            siteObject.search();
        });

        $("#tbl-stream").delegate("tr > td:not(:last-child)", "click", function () {
            var $this = $(this).parents("tr");
            var streamId = $this.attr('data-stream-id');

            if ($this.hasClass('active')) {
                $this.removeClass('active');
            }
            else {
                $this.addClass('active');
            }

            siteObject.UpdateRowActive(streamId);
        });


    };

    siteObject.UpdateRowActive = function (streamId) {
        $.ajax({
            url: "/Home/UpdateActiveRow",
            data: {
                streamId: streamId
            },
            type: 'POST'
        });
    };

    siteObject.search = function () {
        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();
        var league = $('#leagues').val();
        $.ajax({
            url: "/Home/Search",
            data: {
                fromDate: fromDate,
                toDate: toDate,
                league: league
            },
            type: 'POST',
            success: function (data) {
                $("#data-contain").html(data);
            },
            complete: function () {
                siteObject.GetLeagues(league);
                siteObject.AutoRefresh();
                siteObject.Remark();
            }
        });
    };

    siteObject.AutoRefresh = function () {
        var timeOut = parseInt($('#auto-refresh').val());

        setTimeout(function () { SITE.search(); }, timeOut * 60000);
    };

    siteObject.GetLeagues = function (league) {
        $.ajax({
            url: "/Home/GetLeagues",
            type: 'POST',
            success: function (data) {
                $('#leagues').find('option').remove();

                if (league == null) {
                    $('#leagues').append($('<option selected></option>').val('').html('---All Leagues---'));
                }
                else {
                    $('#leagues').append($('<option></option>').val('').html('---All Leagues---'));
                }

                $.each(data, function (index, item) {
                    if (league == item.value) {
                        $('#leagues').append($('<option selected></option>').val(item.value).html(item.name));
                    }
                    else {
                        $('#leagues').append($('<option></option>').val(item.value).html(item.name));
                    }
                });
            }
        });
    };

    siteObject.Remark = function () {
        $("[name=remark]").on('keypress', function (e) {
            if (e.keyCode == 13) {
                var $this = $(this);
                var streamId = $this.parents("tr").attr("data-stream-id");
                var remark = $this.val();

                $.ajax({
                    url:"/Home/UpdateRemark",
                    data: {
                        streamId: streamId,
                        remark: remark
                    },
                    type: 'POST'
                });
            }
        });
    };


    return siteObject;
} (jQuery));