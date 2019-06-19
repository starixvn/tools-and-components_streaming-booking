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

        $('#btnSubmit').on('click', function () {
            siteObject.search();
        });

        $('#providers').on('change', function () {
            $('#licensees').val("");

            siteObject.getLicensees();
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

        $("#sports").prepend('<option selected value="">---All Sports---</option>');


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
        var sport = $('#sports').val();
        console.log('sport', sport)

        $.ajax({
            url: siteObject.getFullUrl("Home/Search"),
            data: {
                fromDate: fromDate,
                toDate: toDate,
                league: league,
                licensee: licensee,
                provider: provider,
                sport: sport
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
        var allDates = $('#fromDate option').map(function (idx, opt) { return $(opt).text(); });
        var fromDate = "", toDate = "";

        if (allDates.length > 0) {
            fromDate = allDates[0];
            toDate = allDates[allDates.length - 1];
        }

        $.ajax({
            url: siteObject.getFullUrl("Home/GetLeagues"),
            data: {
                licensee: licensee,
                provider: provider,
                fromDate: fromDate,
                toDate: toDate
            },
            type: 'GET',
            success: function (data) {
                var $leagues = $('#leagues');

                var leagues = $leagues.find('option').map(function (idx, opt) { return $(opt).text(); }).get();
                var diffs = data.filter(function (i) { return leagues.indexOf(i) < 0; });

                if (diffs.length > 0) {
                    leagues = leagues.concat(diffs);
                    leagues.sort();

                    $leagues.empty();
                    $leagues.append('<option selected value="">---All Leagues---</option>');

                    $.each(leagues, function (index, item) {
                        var newOpt = '<option value="' + item + '">' + item + '</option>';

                        $leagues.append(newOpt);
                    });

                    $leagues.val(league);
                }
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