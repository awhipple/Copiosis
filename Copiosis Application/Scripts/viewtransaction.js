//Function for updating
$("#update").click(function (e) {
    var userType;
    //Check to see what type of user is logged in and update userType
    @if(WebSecurity.CurrentUserId == Model.receiverID)
    {
        <text>userType = 'consumer';</text>
    }
    else
    {
        <text>userType = 'producer';</text>
    }
                    
    var userNotes;
    //Grab our transactionID from the model
    var transGuid = '@Model.transactionID';
    //Determine updated satisfaction rating if any
    var selectedSatisfaction = $('#updateResult').val();
    //Grab the correct notes
    if(userType === "consumer")
        userNotes = $('#receiver-notes').val();
    else
        userNotes = $('#provider-notes').val();
    $.ajax({
        url: "/Account/AddNotes/",
        type: "post",
        //Send the controller the information it needs
        data: { participant: userType, notes: userNotes, tranId: transGuid, newSatisfaction: selectedSatisfaction},
        success: function (result) {
            $("#updateSuccess").removeClass('hidden')
        },
        error: function (result) {
            console.log("Error updating transaction: " + transGuid);
        }
    });
})
@*This is to handle the satisfaction rating*@
$('#satisfaction-rating-radios input:radio').addClass('input_hidden');
$('#satisfaction-rating-radios label').click(function () {
    $(this).addClass('selected').siblings().removeClass('selected');
});

$('#veryUnsatisfiedLabel').click(function () {
    $('#updateResult').val("-2");
})
$('#unsatisfiedLabel').click(function () {
    $('#updateResult').val("-1");
})
$('#neutralLabel').click(function () {
    $('#updateResult').val("0");
})
$('#satisfiedLabel').click(function () {
    $('#updateResult').val("1");
})
$('#verySatisfiedLabel').click(function () {
    $('#updateResult').val("2");
})

$("#reject").click(function (e) {
    $("#result").val("Rejected");
    $("#very-unsatisfied").attr('checked', true);
    $("#ConfirmForm").submit();
})

$("#confirm").click(function (e) {
    $("#result").val("Confirmed");
    $("#ConfirmForm").submit();
})

$('#add-consumer-notes').click(function(){
    $('#receiver-notes').toggleClass('hidden-notes');
})

$('#add-provider-notes').click(function(){
    $('#provider-notes').toggleClass('hidden-notes');
})