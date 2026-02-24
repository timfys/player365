$(document).ready(function(){

     //Toggle 
     $('.showBtn').click(function() {
        //$('.hideme').hide();  
        if ($(this).hasClass('active')) {    
            $(this).removeClass('active');
            $('.hideme').slideUp();
        } else {
            $('.hideme').slideUp();
            $('.showBtn').removeClass('active');
            $(this).addClass('active');
            $(this).next().filter('.hideme').slideDown();
        }
    });

    // Open menu - left side
    $('#showLeft').click(function() {
        $('.menu-left').toggleClass('left-open');
      });
      
      $('.backBtn').click(function() {
        $('.menu').removeClass('left-open');
      });
  
  
      // Open Dashbord Profile
      $('#showRight').click(function() {
        $('.dash-right').toggleClass('right-open');
      });
      
      $('.backBtn').click(function() {
        $('.dash').removeClass('right-open');
      });
    document.querySelectorAll(".accordion_item").forEach(x => x.addEventListener("click", function(){
        if(x.classList.contains("active")){
            x.classList.remove("active");
        }else{
            x.classList.add("active");
        }
    }))
});