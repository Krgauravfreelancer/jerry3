using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.FinalMp4;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO;
using ServerApiCall_UserControl.DTO.VideoEvent;
using ServerApiCall_UserControl.DTO.MediaLibraryModels;
using System.Net.Http;
using System.Windows;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;
using Sqllite_Library.Models;
using Newtonsoft.Json;
using System.Windows.Forms.VisualStyles;
using DesignerNp.controls;
using DebugVideoCreator.Models;

namespace VideoCreator.Auth
{
    public class AuthAPIViewModel : ViewModelBase
    {
        private readonly DateTime MODIFIEDDATE;

        #region === Properties ===

        private readonly VideoCreatorAuthHelper _apiClientHelper;

        private string _projectString;
        public string ProjectString
        {
            get => _projectString;
            set => Set(nameof(ProjectString), ref _projectString, value);
        }

        //private List<ProjectModel> _allProjects;
        //public List<ProjectModel> AllProjects
        //{
        //    get => _allProjects;
        //    set => Set(nameof(AllProjects), ref _allProjects, value);
        //}


        #endregion

        public AuthAPIViewModel()
        {
            _apiClientHelper = new VideoCreatorAuthHelper();
            MODIFIEDDATE = new DateTime(2023, 01, 01);// DateTime.UtcNow.AddDays(-30);
        }

        public bool Isbusy()
        {
            return _apiClientHelper.IsBusy;
        }

        public string GetError()
        {
            return _apiClientHelper.ErrorMessage;
        }

        public string GetToken()
        {
            return _apiClientHelper.TokenNumber;
        }

        public string GetAccessKey()
        {
            return _apiClientHelper.AccessKey;
        }
        public async Task ExecuteLoginAsync()
        {
            await _apiClientHelper.ExecuteLoginAsync();
        }

        public async Task ExecuteLogoutAsync()
        {
            await _apiClientHelper.ExecuteLogoutAsync();
        }


        public string GetLoggedInUser()
        {
            return _apiClientHelper.authCtrl.GetLoggedInUser();
        }

        public async Task<MediaLibraryParent<MediaLibrary>> GetImagesLibraryData(int pagesize = 10, int? page = null, string tags = null)
        {
            var parameters = $"per_page={pagesize}";
            if (page != null)
                parameters += $"&page={page}";
            if (!string.IsNullOrEmpty(tags))
                parameters += $"&tags={tags}";
            var url = $"api/connect/media-library?{parameters}";

            var result = await _apiClientHelper.Get<MediaLibraryParent<MediaLibrary>>(url);
            return result;
        }

        public async Task<List<string>> GetTags()
        {
            var url = $"api/connect/media-library-tags";
            var result = await _apiClientHelper.Get<List<string>>(url);
            return result;
        }


        #region == Project API Calls ==

        public async Task<List<ProjectList>> GetAvailableProjectsData()
        {
            var url = $"api/connect/project/available";
            var result = await _apiClientHelper.Get<ParentDataList<ProjectList>>(url);
            return result?.Data?.Count > 0 ? result.Data : null;
        }

        public async Task<ProjectWithId> GetProjectById(int projectId)
        {
            var url = $"api/connect/project/{projectId}";
            var result = await _apiClientHelper.Get<ParentData<ProjectWithId>>(url);
            return result?.Data != null ? result.Data : default(ProjectWithId);
        }

        //public async Task<ParentData<ProjectAcceptRejectModel>> AcceptOrRejectProject(int project_id, bool accept_flag)
        //{
        //    var url = $"api/connect/project/accept-reject";
        //    var parameters = new Dictionary<string, string>
        //    {
        //        { "project_id", project_id.ToString() },
        //        { "accept_reject", accept_flag ? "1" : "0" }
        //    };
        //    var payload = new FormUrlEncodedContent(parameters);

        //    var result = await _apiClientHelper.Create<ParentData<ProjectAcceptRejectModel>>(url, payload);
        //    return result != null ? result : null;
        //}

        public async Task CreateProject(string project_videotitle, int fk_project_section, int fk_project_projstatus, int project_version, string project_comments)
        {
            var url = $"api/connect/project";
            var parameters = new Dictionary<string, string>
            {
                { "project_videotitle", project_videotitle.ToString() },
                { "fk_project_section", fk_project_section.ToString() },
                { "fk_project_projstatus", fk_project_projstatus.ToString() },
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() },
            };
            var payload = new FormUrlEncodedContent(parameters);
            var result = await _apiClientHelper.Create<ParentData<ProjectList>>(url, payload);
        }

        public async Task UpdateProject(int ProjectId, string project_videotitle, int fk_project_section, int fk_project_projstatus, int project_version, string project_comments)
        {
            var url = $"api/connect/project/{ProjectId}";
            var parameters = new Dictionary<string, string>
            {
                { "project_videotitle", project_videotitle.ToString() },
                { "fk_project_section", fk_project_section.ToString() },
                { "fk_project_projstatus", fk_project_projstatus.ToString() },
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);
            var result = await _apiClientHelper.Update<ParentData<ProjectList>>(url, payload);
        }

        public async Task PatchProject(int projectId, int project_version, string project_comments)
        {
            var url = $"api/connect/project/{projectId}";
            var parameters = new Dictionary<string, string>
            {
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() },
            };
            var payload = new FormUrlEncodedContent(parameters);
            var result = await _apiClientHelper.Patch<ParentData<ProjectList>>(url, payload);
        }

        public async Task GetOwnershipOfProjects(string ProjectIds)
        {
            var url = $@"api/connect/project/owner?project_id={ProjectIds}";
            var result = await _apiClientHelper.Get<Dictionary<string, bool>>(url);
            if (result == null)
                MessageBox.Show("Some Error occured !!!", "GetOwnershipOfProjects", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var builder = new StringBuilder();
                builder.Append("Project Id" + "\t   \t" + "Owner" + "\n");
                builder.Append("==========" + "\t   \t" + "=====" + "\n");
                foreach (var keyPair in result)
                {
                    builder.Append(keyPair.Key + "\t\t : \t" + keyPair.Value + "\n");
                }
                MessageBox.Show(builder.ToString(), "Projects ownership List", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        #endregion

        #region == Master table data ==
        public async Task<AppModel> GetAllApp()
        {
            var url = "api/connect/app";
            var result = await _apiClientHelper.Get<AppModel>(url);
            if (result != null)
                return result;
            return null;
        }

        public async Task<List<MediaModel>> GetAllMedia()
        {
            var url = "api/connect/media";
            var result = await _apiClientHelper.Get<ParentDataList<MediaModel>>(url);
            if (result?.Data?.Count > 0)
                return result.Data;
            return null;
        }

        public async Task<List<ScreenModel>> GetAllScreens()
        {
            var url = "api/connect/screen";
            var result = await _apiClientHelper.Get<ParentDataList<ScreenModel>>(url);
            if (result?.Data?.Count > 0)
                return result.Data;
            return null;
        }

        public async Task<List<CompanyModel>> GetAllCompany()
        {
            var url = $"api/connect/company";

            var result = await _apiClientHelper.Get<List<CompanyModel>>(url);
            if (result != null)
                return result;
            return null;
        }

        public async Task<BackgroundModel> GetAllBackground()
        {
            var url = $"api/connect/background";
            var result = await _apiClientHelper.Get<ParentData<BackgroundModel>>(url);
            if (result != null)
                return result?.Data;
            //else
            //    MessageBox.Show($"No Background Found", "Synchronising Background Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<string> DownloadBackground(int backgroundId, string extension)
        {
            var url = $"api/connect/background/download/{backgroundId}";
            var filename = $"background_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            var success = await _apiClientHelper.GetFile(url, filename);
            if (success)
            {
                var filepath = $@"{Directory.GetCurrentDirectory()}\\{filename}";
                return filepath;
            }
            return null;
        }

        public async Task<byte[]> GetSecuredFileByteArray(string url)
        {
            var result = await _apiClientHelper.GetSecuredFileByteArray(url);
            return result;
        }


        #endregion
        /*
        #region == Company ==
        public async Task CreateCompany(string company_name)
        {
            var url = $"api/connect/company";
            var parameters = new Dictionary<string, string>
            {
                { "company_name", company_name }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateCompany", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateCompany", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateCompany(int companyId, string company_name)
        {
            var url = $"api/connect/company/{companyId}";
            var parameters = new Dictionary<string, string>
            {
                { "company_name", company_name }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateCompany", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateCompany", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
        */
        /*
        #region == Background ==

        
        //public async Task ListBackgroundById(int backgroundId)
        //{
        //    //var url = $"api/connect/background/download/{backgroundId}";
        //    //var result = await _apiClientHelper.Get<ParentData<BackgroundModel>>(url);
        //    //if (result != null)
        //    //{
        //    //    MessageBox.Show(result.Data.ToString(), $"ListBackgroundById - For {backgroundId}", MessageBoxButton.OK, MessageBoxImage.Information);
        //    //}
        //    //else
        //    //{
        //    //    MessageBox.Show($"No Background Found", "ListBackgroundById", MessageBoxButton.OK, MessageBoxImage.Error);
        //    //}
        //}

        public async Task CreateBackground(int fk_backgrounds_company, int backgrounds_active, string backgrounds_media)
        {
            var url = $"api/connect/background";
            var multipart = new MultipartFormDataContent();
            // FK
            var requestbodyContent = new StringContent(fk_backgrounds_company.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"fk_backgrounds_company\"");
            multipart.Add(requestbodyContent);

            // FK
            requestbodyContent = new StringContent(backgrounds_active.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"backgrounds_active\"");
            multipart.Add(requestbodyContent);

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{backgrounds_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"backgrounds_media\"; filename=\"{backgrounds_media}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<BackgroundModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateBackground", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateBackground(int backgroundId, int fk_backgrounds_company, int backgrounds_active)
        {
            var url = $"api/connect/background/{backgroundId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_backgrounds_company", fk_backgrounds_company.ToString() },
                { "backgrounds_active", backgrounds_active.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateBackground", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateBackground", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchBackground(int projectId, int videoeventId, int fk_videoevent_media, string videoevent_start, int videoevent_duration)
        {
            var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videoevent_media", fk_videoevent_media.ToString() },
                { "videoevent_start", videoevent_start },
                { "videoevent_duration", videoevent_duration.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<VideoEventModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DownloadBackground(int backgroundId, string extension)
        {
            var url = $"api/connect/background/download/{backgroundId}";
            var filename = $"background_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadBackground", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion
        */
        #region == Video Event ==

        public async Task<ParentDataList<AllVideoEventResponseModel>> GetAllVideoEventsbyProjdetId(SelectedProjectEvent selectedProjectEvent)
        {
            var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent";

            var result = await _apiClientHelper.Get<ParentDataList<AllVideoEventResponseModel>>(url);
            if (result != null)
                return result;
            return null;
        }

        public async Task<VideoEventResponseModel> POSTVideoEvent(SelectedProjectEvent selectedProjectEvent, VideoEventModel videoEventModel)
        {
            try
            {
                //_apiClientHelper.ErrorMessage = "No Internet !!";
                //throw new Exception("No Internet !!");
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent-notes-design-videosegment";

                var multipart = new MultipartFormDataContent();
                // FK
                var requestbodyContent_FK = new StringContent(videoEventModel.fk_videoevent_media.ToString());
                requestbodyContent_FK.Headers.Add("Content-Disposition", "form-data; name=\"fk_videoevent_media\"");
                multipart.Add(requestbodyContent_FK);
                // Track
                var requestbodyContent_track = new StringContent(videoEventModel.videoevent_track.ToString());
                requestbodyContent_track.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_track\"");
                multipart.Add(requestbodyContent_track);
                // Start
                var requestbodyContent_start = new StringContent(videoEventModel.videoevent_start.ToString());
                requestbodyContent_start.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_start\"");
                multipart.Add(requestbodyContent_start);
                // Duration
                var requestbodyContent_duration = new StringContent(videoEventModel.videoevent_duration.ToString());
                requestbodyContent_duration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_duration\"");
                multipart.Add(requestbodyContent_duration);
                // Start
                var requestbodyContent_end = new StringContent(videoEventModel.videoevent_end.ToString());
                requestbodyContent_end.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_end\"");
                multipart.Add(requestbodyContent_end);
                // LOC DATE
                var requestbodyContent_LOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
                requestbodyContent_LOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_modifylocdate\"");
                multipart.Add(requestbodyContent_LOCDATE);



                // notes
                if (videoEventModel.notes?.Count > 0)
                {
                    var requestbodyContent_notes = new StringContent(JsonConvert.SerializeObject(videoEventModel.notes));
                    requestbodyContent_notes.Headers.Add("Content-Disposition", "form-data; name=\"notes\"");
                    multipart.Add(requestbodyContent_notes);
                }
                // design
                if (videoEventModel.design?.Count > 0)
                {
                    var requestbodyContent_design = new StringContent(JsonConvert.SerializeObject(videoEventModel.design));
                    requestbodyContent_design.Headers.Add("Content-Disposition", "form-data; name=\"design\"");
                    multipart.Add(requestbodyContent_design);
                }

                string pathWithFilename = string.Empty;
                StreamContent fileStreamContent = null;
                FileStream fileReadStream = null;
                if (videoEventModel.videosegment_media_bytes?.Length > 0)
                {
                    //File
                    var filename = $"_{Guid.NewGuid()}";
                    if (videoEventModel.fk_videoevent_media == (int)EnumMedia.VIDEO)
                        filename = "video" + filename + ".mp4";
                    else if (videoEventModel.fk_videoevent_media == (int)EnumMedia.IMAGE)
                        filename = "image" + filename + ".png";
                    else if (videoEventModel.fk_videoevent_media == (int)EnumMedia.FORM)
                        filename = "design" + filename + ".png";

                    var currentDirectory = Directory.GetCurrentDirectory();
                    pathWithFilename = $"{currentDirectory}\\Media\\{filename}";
                    var file = new FileStream(pathWithFilename, FileMode.OpenOrCreate, FileAccess.Write);
                    file.Write(videoEventModel.videosegment_media_bytes, 0, videoEventModel.videosegment_media_bytes.Length);
                    file.Close();

                    fileReadStream = new FileStream(pathWithFilename, FileMode.Open);
                    fileStreamContent = new StreamContent(fileReadStream);
                    fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{filename}\"");
                    multipart.Add(fileStreamContent);


                    // LOC DATE
                    var requestbodyContent_VideoSegmentLOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
                    requestbodyContent_VideoSegmentLOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videosegment_modifylocdate\"");
                    multipart.Add(requestbodyContent_VideoSegmentLOCDATE);
                }

                var result = await _apiClientHelper.CreateWithMultipart<ParentData<VideoEventResponseModel>>(url, multipart);
                //if (result?.Status == "success")
                //    MessageBox.Show($@"{result?.Message} with data - {JsonConvert.SerializeObject(result.Data)}", "CreateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
                if (pathWithFilename?.Length > 0)
                {

                    fileReadStream?.Close();
                    fileStreamContent.Dispose();
                    File.Delete(pathWithFilename);
                }
                return result?.Data;
            }
            catch { return null; }
        }

        public async Task<VideoEventModel> PutVideoEvent(SelectedProjectEvent selectedProjectEvent, Int64 selectedServerVideoEventId, VideoEventModel videoEventModel)
        {
            try
            {
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent/{selectedServerVideoEventId}";
                var parameters = new Dictionary<string, string>
                {
                    { "fk_videoevent_media", videoEventModel.fk_videoevent_media.ToString() },
                    { "videoevent_track", videoEventModel.videoevent_track.ToString() },
                    { "videoevent_start", videoEventModel.videoevent_start.ToString() },
                    { "videoevent_duration", videoEventModel.videoevent_duration.ToString() },
                    { "videoevent_end", videoEventModel.videoevent_end.ToString() },
                    { "videoevent_modifylocdate", videoEventModel.videoevent_modifylocdate.ToString() }
                };
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Update<ParentData<VideoEventModel>>(url, payload);
                return result?.Data;
            }
            catch { return null; }
        }


        public async Task<ParentData<VideoSegmentPostResponse>> PostVideoSegment(int videoevent_serverid, EnumMedia MediaType, byte[] blob = null)
        {
            var url = $"api/connect/videoevent/{videoevent_serverid}/videosegment-create";
            var multipart = new MultipartFormDataContent();
            string pathWithFilename = string.Empty;
            StreamContent fileStreamContent = null;
            FileStream fileReadStream = null;
            if (blob?.Length > 0)
            {
                //File
                var filename = $"{Guid.NewGuid()}";
                if (MediaType == EnumMedia.VIDEO)
                    filename = $"video_{filename}.mp4";
                else if (MediaType == EnumMedia.IMAGE)
                    filename = $"image_{filename}.png";
                else if (MediaType == EnumMedia.FORM)
                    filename = $"design_{filename}.png";

                var currentDirectory = Directory.GetCurrentDirectory();
                pathWithFilename = $"{currentDirectory}\\Media\\{filename}";
                var file = new FileStream(pathWithFilename, FileMode.OpenOrCreate, FileAccess.Write);
                file.Write(blob, 0, blob.Length);
                file.Close();

                fileReadStream = new FileStream(pathWithFilename, FileMode.Open);
                fileStreamContent = new StreamContent(fileReadStream);
                fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{filename}\"");
                multipart.Add(fileStreamContent);
            }

            var result = await _apiClientHelper.CreateWithMultipart<ParentData<VideoSegmentPostResponse>>(url, multipart);
            //if (result?.Status == "success")
            //    MessageBox.Show($@"{result?.Message} with data - {JsonConvert.SerializeObject(result.Data)}", "PostVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            if (pathWithFilename?.Length > 0)
            {

                fileReadStream?.Close();
                fileStreamContent.Dispose();
                File.Delete(pathWithFilename);
            }
            return result;
        }

        public async Task<NotesResponseModel> POSTNotes(Int64 selectedServerVideoEventId, List<NotesModelPost> notes)
        {
            var url = $"api/connect/videoevent/{selectedServerVideoEventId}/notes";
            var parameters = new Dictionary<string, string>
            {
                { "notes", JsonConvert.SerializeObject(notes) }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var response = await _apiClientHelper.Create<ParentData<NotesResponseModel>>(url, payload);
            return response?.Data;
        }

        public async Task<ParentData<object>> DeleteNotesById(Int64 selectedServerVideoEventId, Int64 notesId)
        {
            var url = $"api/connect/videoevent/{selectedServerVideoEventId}/notes/{notesId}";
            var parameters = new Dictionary<string, string>();

            var payload = new FormUrlEncodedContent(parameters);

            var response = await _apiClientHelper.Delete<ParentData<object>>(url, payload);
            return response;
        }



        public async Task<VideoEventResponseModel> PutVideoEvent(int projectId, VideoEventModel videoEventModel, byte[] blob = null)
        {
            var url = $"api/connect/project/{projectId}/videoevent-update/{(int)videoEventModel.videoevent_serverid}";

            var multipart = new MultipartFormDataContent();
            // FK
            var requestbodyContent_FK = new StringContent(videoEventModel.fk_videoevent_media.ToString());
            requestbodyContent_FK.Headers.Add("Content-Disposition", "form-data; name=\"fk_videoevent_media\"");
            multipart.Add(requestbodyContent_FK);
            // Track
            var requestbodyContent_track = new StringContent(videoEventModel.videoevent_track.ToString());
            requestbodyContent_track.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_track\"");
            multipart.Add(requestbodyContent_track);
            // Start
            var requestbodyContent_start = new StringContent(videoEventModel.videoevent_start.ToString());
            requestbodyContent_start.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_start\"");
            multipart.Add(requestbodyContent_start);
            // Duration
            var requestbodyContent_duration = new StringContent(videoEventModel.videoevent_duration.ToString());
            requestbodyContent_duration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_duration\"");
            multipart.Add(requestbodyContent_duration);
            // LOC DATE
            //var requestbodyContent_LOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
            //requestbodyContent_LOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_modifylocdate\"");
            //multipart.Add(requestbodyContent_LOCDATE);

            // notes
            if (videoEventModel.notes?.Count > 0)
            {
                var requestbodyContent_notes = new StringContent(JsonConvert.SerializeObject(videoEventModel.notes));
                requestbodyContent_notes.Headers.Add("Content-Disposition", "form-data; name=\"notes\"");
                multipart.Add(requestbodyContent_notes);
            }
            // design
            if (videoEventModel.design?.Count > 0)
            {
                var requestbodyContent_design = new StringContent(JsonConvert.SerializeObject(videoEventModel.design));
                requestbodyContent_design.Headers.Add("Content-Disposition", "form-data; name=\"design\"");
                multipart.Add(requestbodyContent_design);
            }

            string pathWithFilename = string.Empty;
            StreamContent fileStreamContent = null;
            FileStream fileReadStream = null;
            if (blob?.Length > 0)
            {
                //File
                var filename = $"design_{Guid.NewGuid()}.png";

                var currentDirectory = Directory.GetCurrentDirectory();
                pathWithFilename = $"{currentDirectory}\\Media\\{filename}";
                var file = new FileStream(pathWithFilename, FileMode.OpenOrCreate, FileAccess.Write);
                file.Write(blob, 0, blob.Length);
                file.Close();

                fileReadStream = new FileStream(pathWithFilename, FileMode.Open);
                fileStreamContent = new StreamContent(fileReadStream);
                fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{filename}\"");
                multipart.Add(fileStreamContent);
            }

            var result = await _apiClientHelper.CreateWithMultipart<ParentData<VideoEventResponseModel>>(url, multipart);
            if (result?.Status == "success")
                MessageBox.Show($@"{result?.Message} with data - {JsonConvert.SerializeObject(result.Data)}", "CreateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            if (pathWithFilename?.Length > 0)
            {

                fileReadStream?.Close();
                fileStreamContent.Dispose();
                File.Delete(pathWithFilename);
            }
            return result?.Data;
        }






        //public async Task ListAllVideoEvent(DateTime? modifiedDate = null)
        //{
        //    var url = $"api/connect/project/2/videoevent";
        //    if (modifiedDate.HasValue)
        //        url += $"?modifydate={modifiedDate:yyyy-MM-dd HH:mm:ss}";

        //    var result = await _apiClientHelper.Get<ParentDataList<VideoEventModel>>(url);
        //    if (result != null)
        //    {
        //        var builder = new StringBuilder();
        //        builder.Append($"Total Record found - {result.Data.Count}" + "\n");
        //        foreach (var keyPair in result.Data)
        //            builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
        //        MessageBox.Show(builder.ToString(), $"List All Records - From {modifiedDate}", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        MessageBox.Show($"No Record Found", "ListAllVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //public async Task ListVideoEventById(int projectId, int videoEventId)
        //{
        //    var url = $"api/connect/project/{projectId}/videoevent/{videoEventId}";
        //    var result = await _apiClientHelper.Get<ParentData<VideoEventModel>>(url);
        //    if (result != null)
        //    {
        //        MessageBox.Show(result.Data.ToString(), $"ListVideoEventById - For {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        MessageBox.Show($"No Record Found", "ListVideoEventById", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}



        //public async Task UpdateVideoEvent(int projectId, int videoeventId, int fk_videoevent_media, int videoevent_track, string videoevent_start, int videoevent_duration)
        //{
        //    var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
        //    var parameters = new Dictionary<string, string>
        //    {
        //        { "fk_videoevent_media", fk_videoevent_media.ToString() },
        //        { "videoevent_track", videoevent_track.ToString() },
        //        { "videoevent_start", videoevent_start },
        //        { "videoevent_duration", videoevent_duration.ToString() }
        //    };
        //    var payload = new FormUrlEncodedContent(parameters);

        //    var result = await _apiClientHelper.Update<ParentData<VideoEventModel>>(url, payload);
        //    if (result?.Status == "success")
        //    {
        //        MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        MessageBox.Show(result?.Message, "UpdateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //public async Task PatchVideoEvent(int projectId, int videoeventId, int fk_videoevent_media, string videoevent_start, int videoevent_duration)
        //{
        //    var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
        //    var parameters = new Dictionary<string, string>
        //    {
        //        { "fk_videoevent_media", fk_videoevent_media.ToString() },
        //        { "videoevent_start", videoevent_start },
        //        { "videoevent_duration", videoevent_duration.ToString() }
        //    };
        //    var payload = new FormUrlEncodedContent(parameters);

        //    var result = await _apiClientHelper.Patch<ParentData<VideoEventModel>>(url, payload);
        //    if (result?.Status == "success")
        //    {
        //        MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
        //    }
        //    else
        //    {
        //        MessageBox.Show(result?.Message, "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        #endregion
        /*
        #region == Audio ==

        public async Task ListAllAudio(int videoEventId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/audio";

            var result = await _apiClientHelper.Get<ParentDataList<AudioModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Audio found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Audio for Video Event - {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Audio Found", "ListAllAudio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetAudioById(int videoEventId, int audioId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/audio/{audioId}";

            var result = await _apiClientHelper.Get<ParentData<AudioModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetAudioById - For {videoEventId} videoevent in audio {audioId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetAudioById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateAudio(int fk_audio_videoevent, string filename)
        {
            var url = $"api/connect/videoevent/{fk_audio_videoevent}/audio";

            var multipart = new MultipartFormDataContent();
            // FK
            var requestbodyContent = new StringContent(fk_audio_videoevent.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"fk_audio_videoevent\"");
            multipart.Add(requestbodyContent);

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{filename}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"audio_media\"; filename=\"{filename}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<AudioModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateAudio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateAudio(int videoeventId, int audioId, string filename)
        {
            var url = $"api/connect/videoevent/{videoeventId}/audio/{audioId}";

            var multipart = new MultipartFormDataContent();

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{filename}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"audio_media\"; filename=\"{filename}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<AudioModel>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<AudioModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateAudio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }


        public async Task DownloadAudioBinary(int audioId, string extension)
        {
            var url = $"api/connect/download/audio/{audioId}";
            var filename = $"audio_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadAudioBinary", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion

        #region == VideoSegments ==

        public async Task ListAllVideoSegments(int videoEventId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/videosegment";

            var result = await _apiClientHelper.Get<ParentDataList<VideoSegmentModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total VideoSegments found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"ListAllVideoSegments - {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Video Segment Found", "ListAllVideoSegments", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetVideoSegmentById(int videoEventId, int videosegmentId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/videosegment/{videosegmentId}";

            var result = await _apiClientHelper.Get<ParentData<VideoSegmentModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetVideoSegmentById - For {videoEventId} videoevent in videosegment {videosegmentId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetVideoSegmentById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateVideoSegmentBinary(int videoeventId, string videosegment_media)
        {
            var url = $"api/connect/videoevent/{videoeventId}/videosegment";

            var multipart = new MultipartFormDataContent();


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{videosegment_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{videosegment_media}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<VideoSegmentModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateVideoSegmentBinary(int videoeventId, int videosegmentId, string videosegment_media)
        {
            var url = $"api/connect/videoevent/{videoeventId}/videosegment/{videosegmentId}";

            var multipart = new MultipartFormDataContent();

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{videosegment_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{videosegment_media}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<AudioModel>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<VideoSegmentModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "UpdateVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
        }

        public async Task DownloadBinaryVideoSegment(int videosegmentId, string extension)
        {
            var url = $"api/connect/download/videosegment/{videosegmentId}";
            var filename = $"videosegment_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadBackground", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task PatchVideoSegment(int fk_videosegment_videoevent, int videosegmentId)
        {
            var url = $"api/connect/videoevent/{fk_videosegment_videoevent}/videosegment/{videosegmentId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videosegment_videoevent", fk_videosegment_videoevent.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<VideoSegmentModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchVideoSegment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Design ==

        public async Task ListAllDesigns(int videoevent, DateTime? modifiedDate = null)
        {
            var url = $"api/connect/videoevent/{videoevent}/design";
            if (modifiedDate.HasValue)
                url += $"?modifydate={modifiedDate:yyyy-MM-dd HH:mm:ss}";

            var result = await _apiClientHelper.Get<ParentDataList<DesignModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Record found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Record - From {modifiedDate}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListAllDesigns", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ListDesignById(int videoEventId, int designId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/design/{designId}";
            var result = await _apiClientHelper.Get<ParentData<DesignModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"ListDesignById - For {designId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListDesignById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateDesign(int videoeventId, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateDesign(int videoeventId, int designId, int fk_design_videoevent, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design/{designId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "fk_design_videoevent", fk_design_videoevent.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchDesign(int videoeventId, int designId, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design/{designId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Notes ==

        public async Task ListAllNotes(int videoeventId)
        {
            var url = $"api/connect/videoevent/{videoeventId}/notes";

            var result = await _apiClientHelper.Get<ParentDataList<NotesModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Notes found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Notes - For videoevent {videoeventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListAllNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetNotesById(int videoeventId, int notesId)
        {
            var url = $"api/connect/videoevent/{videoeventId}/notes/{notesId}";
            var result = await _apiClientHelper.Get<ParentData<NotesModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetNotesById - For {notesId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetNotesById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateNotes(int fk_notes_videoevent, string notes_line, int notes_wordcount, int notes_index)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes";
            var parameters = new Dictionary<string, string>
            {
                { "fk_notes_videoevent", fk_notes_videoevent.ToString() },
                { "notes_line", notes_line.ToString() },
                { "notes_wordcount", notes_wordcount.ToString() },
                { "notes_index", notes_index.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateNotes(int notesId, int fk_notes_videoevent, string notes_line, int notes_wordcount, int notes_index)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes/{notesId}";
            var parameters = new Dictionary<string, string>
            {
                { "notes_wordcount", notes_wordcount.ToString() },
                { "notes_line", notes_line },
                { "notes_index", notes_index.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchNotes(int notesId, int fk_notes_videoevent, string notes_line)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes/{notesId}";
            var parameters = new Dictionary<string, string>
            {
                { "notes_line", notes_line.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == FINALMP4 ==

        public async Task GetFinalMp4ById(int projectId, int finalmp4Id)
        {
            var url = $"api/connect/mp4/{projectId}/video/{finalmp4Id}";
            var result = await _apiClientHelper.Get<FinalMp4Model>(url);
            if (result != null)
            {
                MessageBox.Show(result.ToString(), $"GetFinalMp4ById - For {finalmp4Id}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetFinalMp4ById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateFinalMp4(int projectId, int finalmp4_version, string finalmp4_comments, string finalmp4_mediamp4)
        {
            var url = $"api/connect/mp4/{projectId}/video";

            var multipart = new MultipartFormDataContent();


            // FK
            var requestbodyContent = new StringContent(finalmp4_version.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_version\"");
            multipart.Add(requestbodyContent);


            // Comments
            requestbodyContent = new StringContent(finalmp4_comments.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_comments\"");
            multipart.Add(requestbodyContent);


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{finalmp4_mediamp4}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"finalmp4_mediamp4\"; filename=\"{finalmp4_mediamp4}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<FinalMp4Model>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }


        public async Task UpdateFinalMp4(int projectId, int mp4id, int finalmp4_version, string finalmp4_comments, string finalmp4_mediamp4)
        {
            var url = $"api/connect/mp4/{projectId}/videoupdate/{mp4id}";

            var multipart = new MultipartFormDataContent();


            // FK
            var requestbodyContent = new StringContent(finalmp4_version.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_version\"");
            multipart.Add(requestbodyContent);


            // Comments
            requestbodyContent = new StringContent(finalmp4_comments.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_comments\"");
            multipart.Add(requestbodyContent);


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{finalmp4_mediamp4}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"finalmp4_mediamp4\"; filename=\"{finalmp4_mediamp4}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<FinalMp4Model>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<FinalMp4Model>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task DownloadFinalMp4(int FinalMp4Id, string extension)
        {
            var url = $"api/connect/download/video/mp4/{FinalMp4Id}";
            var filename = $"finalmp4_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion
        */
    }
}
