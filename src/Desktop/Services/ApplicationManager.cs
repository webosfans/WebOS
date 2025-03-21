using System.Collections.ObjectModel;
using Desktop.DTOs;

namespace Desktop.Services;

public class ApplicationManager
{
    public ObservableCollection<AppDto> PinedApplications { get; } = new ();

    public ApplicationManager()
    {
        PinedApplications.Add(new AppDto("News", "/assets/img/app_icons/news@256x256.png", "/apps/news")
        {
            DefaultWidth = 960
        });
    }   
}