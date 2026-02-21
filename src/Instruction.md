# Instruction

Follow this instruction to complete the requirement

## Project information

- This project have 3 cores functionalities:
  - A landing page (**leave for later**)
  - An online resource hub for teacher (**focus this part**)
  - A portal for school staff to setup timetable, view check in,... (**focus later**)
- The project involves around a platform to manage a STEM course for preschool kids, where the learning tools is a cardboard card.
- For stakeholders, we have the admin, staff (platform staff -> manager learning resource in the hub), school (to create timetable, view check in, get report,...), teacher (to view learning resources, view timetable, check in). Because we focus solely in the second functionality, aside from admin and staff, other role only view learning resource. **There is no Guest here**
- Because of the characteristics of this project, a register function wouldn't be provided, it would be the admin who create account, and other can only login, so keep that in mind (though it wouldn't be matter much in this phase)
- The database use is `PostgreSQL`

## Requirements

- From the View Controller in `./Controllers/Views/`, implement the UI. Use `FrankenUI` using `CDN`.
- Use the authorization policies described above to judge what the dashboard should include

> [!IMPORTANT]
> Most of the logic is completed, so do not dwell too much on them, although you should look at `./Dtos/` and `./Services/AuthService.cs` and `./Shared/Exceptions/` to understand what they do
> Logout will be handle by the UI (remove token out of cookie). There is no blacklist mechanism yet in this page
> The admin/staff dashboard should be easy to use but still look modern. As for the Hub, it should look modern, like what most of the online learning page should look like
