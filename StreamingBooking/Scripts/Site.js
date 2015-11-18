var SITE = (function ($) {
    var siteObject = {};
    siteObject.initialize = function (baseUrl) {
        siteObject.baseUrl = baseUrl;

        $(document).ajaxStart(function () {
            $('#ajax_loader').show();

        }).ajaxStop(function () {
            $('#ajax_loader').hide();
        }).ajaxError(function (xhr, status) {
            $("#ajax-debug").text(status.responseText);
            alert("There is an ajax error, please view ajax-debug content for further information");
        });

        $('#fromDate').on('change', function () {
            siteObject.search();
        });

        $('#toDate').on('change', function () {
            siteObject.search();
        });

        $('#leagues').on('change', function () {
            siteObject.search();
        });

        $('#licensees').on('change', function () {
            $('#leagues').val("");

            siteObject.search();
        });

        $('#providers').on('change', function () {
            $('#licensees').val("");
            $('#leagues').val("");

            siteObject.getLicensees();

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

        $("#providers").prepend('<option selected value="0">---All Providers---</option>');

        $("#licensees").prepend('<option selected value="">---All Licensees---</option>');

        siteObject.search(true);
    };

    siteObject.getFullUrl = function (relativePath) {
        var fullUrl = siteObject.baseUrl + relativePath;

        return fullUrl;
    };

    siteObject.UpdateRowActive = function (streamId) {
        $.ajax({
            url: siteObject.getFullUrl("Home/UpdateActiveRow"),
            data: {
                streamId: streamId
            },
            type: 'POST'
        });
    };

    siteObject.search = function (isAutoRefresh) {
        var fromDate = $('#fromDate').val();
        var toDate = $('#toDate').val();
        var league = $('#leagues').val();
        var licensee = $('#licensees').val();
        var provider = $('#providers').val();

        $.ajax({
            url: siteObject.getFullUrl("Home/Search"),
            data: {
                fromDate: fromDate,
                toDate: toDate,
                league: league,
                licensee: licensee,
                provider: provider
            },
            type: 'POST',
            success: function (data) {
                $("#data-contain").html(data);
            },
            complete: function () {
                siteObject.GetLeagues(league);
                siteObject.Remark();

                if (isAutoRefresh) {
                    siteObject.AutoRefresh();
                }
            }
        });
    };

    siteObject.AutoRefresh = function () {
        var timeOut = parseInt($('#auto-refresh').val()),
            isAutoRefresh = true;

        setTimeout(function () { SITE.search(isAutoRefresh); }, timeOut * 60000);
    };

    siteObject.GetLeagues = function (league) {
        var licensee = $('#licensees').val();
        var provider = $('#providers').val();

        $.ajax({
            url: siteObject.getFullUrl("Home/GetLeagues"),
            data: {
                licensee: licensee,
                provider: provider
            },
            type: 'GET',
            success: function (data) {
                $('#leagues').find('option').remove();

                if (league == null) {
                    $('#leagues').append($('<option selected></option>').val('').html('---All Leagues---'));
                }
                else {
                    $('#leagues').append($('<option></option>').val('').html('---All Leagues---'));
                }

                $.each(data, function (index, item) {
                    if (league == item) {
                        $('#leagues').append($('<option selected></option>').val(item).html(item));
                    }
                    else {
                        $('#leagues').append($('<option></option>').val(item).html(item));
                    }
                });
            }
        });
    };

    siteObject.getLicensees = function () {
        var provider = $('#providers').val();

        $.ajax({
            url: siteObject.getFullUrl("Home/GetLicenseesByProvider"),
            data: {
                provider: provider
            },
            type: 'GET',
            success: function (data) {
                $('#licensees').find('option').remove();

                $('#licensees').append($('<option selected></option>').val('').html('---All Licensees---'));

                $.each(data, function (index, item) {
                    $('#licensees').append($('<option></option>').val(item).html(item));
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
                    url: siteObject.getFullUrl("Home/UpdateRemark"),
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