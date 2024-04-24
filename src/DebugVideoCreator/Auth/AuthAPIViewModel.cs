using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using ServerApiCall_UserControl.DTO;
using ServerApiCall_UserControl.DTO.App;
using ServerApiCall_UserControl.DTO.Background;
using ServerApiCall_UserControl.DTO.Company;
using ServerApiCall_UserControl.DTO.Media;
using ServerApiCall_UserControl.DTO.MediaLibraryModels;
using ServerApiCall_UserControl.DTO.Projects;
using ServerApiCall_UserControl.DTO.Screen;
using ServerApiCall_UserControl.DTO.VideoEvent;
using Sqllite_Library.Helpers;
using Sqllite_Library.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VideoCreator.Helpers;

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

        public async Task<string> SubmitProject(SelectedProjectEvent selectedProjectEvent)
        {
            var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/submit";
            var parameters = new Dictionary<string, string>();
            var payload = new FormUrlEncodedContent(parameters);
            var result = await _apiClientHelper.Update<ParentData<object>>(url, payload);
            return result?.Message;
        }
        
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

        #region  == Planning ==

        public async Task<List<PlanningModel>> GetPlanningsByProjectId(int projectId)
        {
            var url = $"api/connect/project/{projectId}/planning";
            var result = await _apiClientHelper.Get<ParentData<List<PlanningModel>>>(url);
            return result?.Data != null ? result.Data : null;
        }

        public async Task<bool> IsPlanningUpdated(int projectId)
        {
            var url = $"api/connect/project/{projectId}/last-updates";
            var result = await _apiClientHelper.Get<LastUpdatedPlannigModelResponse>(url);
            return result?.planning?.newupdates ?? false;
        }

        public async Task<bool> UpdatedPlanningLastUpdated(int projectId)
        {
            try
            {
                var url = $"api/connect/project/{projectId}/last-updates";
                var parameters = new Dictionary<string, string>
                {
                    { "planning", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") }
                };
                var payload = new FormUrlEncodedContent(parameters);
                var result = await _apiClientHelper.Update<object>(url, payload);
                return true;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return false;
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

        #endregion

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
                var requestbodyContent_duration = new StringContent(videoEventModel.videoevent_duration);
                requestbodyContent_duration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_duration\"");
                multipart.Add(requestbodyContent_duration);
                // OrigDuration
                var requestbodyContent_origduration = new StringContent(videoEventModel.videoevent_origduration);
                requestbodyContent_origduration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_origduration\"");
                multipart.Add(requestbodyContent_origduration);
                // Start
                var requestbodyContent_end = new StringContent(videoEventModel.videoevent_end.ToString());
                requestbodyContent_end.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_end\"");
                multipart.Add(requestbodyContent_end);
                // LOC DATE
                var requestbodyContent_LOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
                requestbodyContent_LOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_modifylocdate\"");
                multipart.Add(requestbodyContent_LOCDATE);
                // Video Event Planning
                var requestbodyContent_PLANNING = new StringContent(videoEventModel.videoevent_planning.ToString());
                requestbodyContent_PLANNING.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_planning\"");
                multipart.Add(requestbodyContent_PLANNING);



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

                    var temp = PathHelper.GetTempPath("videoevent");
                    pathWithFilename = $"{temp}\\{filename}";
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
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
        }

        public async Task<VideoEventResponseModel> PutVideoEventForDesign(SelectedProjectEvent selectedProjectEvent, VideoEventModel videoEventModel, Int64 selectedServerVideoEventId)
        {
            try
            {
                //_apiClientHelper.ErrorMessage = "No Internet !!";
                //throw new Exception("No Internet !!");
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent-notes-design-videosegment/{selectedServerVideoEventId}";
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
                var requestbodyContent_duration = new StringContent(videoEventModel.videoevent_duration);
                requestbodyContent_duration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_duration\"");
                multipart.Add(requestbodyContent_duration);
                // OrigDuration
                var requestbodyContent_origduration = new StringContent(videoEventModel.videoevent_origduration);
                requestbodyContent_origduration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_origduration\"");
                multipart.Add(requestbodyContent_origduration);
                // Start
                var requestbodyContent_end = new StringContent(videoEventModel.videoevent_end.ToString());
                requestbodyContent_end.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_end\"");
                multipart.Add(requestbodyContent_end);
                // LOC DATE
                var requestbodyContent_LOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
                requestbodyContent_LOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_modifylocdate\"");
                multipart.Add(requestbodyContent_LOCDATE);
                // Video Event Planning
                var requestbodyContent_PLANNING = new StringContent(videoEventModel.videoevent_planning.ToString());
                requestbodyContent_PLANNING.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_planning\"");
                multipart.Add(requestbodyContent_PLANNING);



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

                    var temp = PathHelper.GetTempPath("videoevent");
                    pathWithFilename = $"{temp}\\{filename}";
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

                var result = await _apiClientHelper.UpdateWithMultipart<ParentData<VideoEventResponseModel>>(url, multipart);
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
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
        }

        public async Task<VideoEventModel> UpdateVideoEventOnlyToServer(SelectedProjectEvent selectedProjectEvent, Int64 selectedServerVideoEventId, VideoEventModel videoEventModel)
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
                    { "videoevent_origduration", videoEventModel.videoevent_origduration.ToString() },
                    { "videoevent_end", videoEventModel.videoevent_end.ToString() },
                    { "videoevent_modifylocdate", videoEventModel.videoevent_modifylocdate.ToString() }
                };
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Update<ParentData<VideoEventModel>>(url, payload);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
        }

        public async Task<VideoEventResponseModel> DeleteVideoEvent(SelectedProjectEvent selectedProjectEvent, Int64 videoevent_serverId)
        {
            try
            {
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent/{videoevent_serverId}";
                var parameters = new Dictionary<string, string>();
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Delete<ParentData<VideoEventResponseModel>>(url, payload);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
        }

        public async Task<object> UndeleteVideoEventToServer(SelectedProjectEvent selectedProjectEvent, Int64 videoevent_serverId)
        {
            try
            {
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent/{videoevent_serverId}/restore";
                var parameters = new Dictionary<string, string>();
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Update<ParentData<object>>(url, payload);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                return null;
            }
        }

        public async Task<List<ShiftVideoEventModel>> ShiftVideoEvent(SelectedProjectEvent selectedProjectEvent, List<ShiftVideoEventModel> shiftVideoEvents)
        {
            try
            {
                var url = $"api/connect/project/{selectedProjectEvent.serverProjectId}/project-detail/{selectedProjectEvent.serverProjdetId}/videoevent-shift";
                var parameters = new Dictionary<string, string>
                {
                    { "videoevent", JsonConvert.SerializeObject(shiftVideoEvents) }
                };
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Update<ParentData<List<ShiftVideoEventModel>>>(url, payload);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
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
            var requestbodyContent_duration = new StringContent(videoEventModel.videoevent_duration);
            requestbodyContent_duration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_duration\"");
            multipart.Add(requestbodyContent_duration);
            // OrigDuration
            var requestbodyContent_origduration = new StringContent(videoEventModel.videoevent_origduration);
            requestbodyContent_origduration.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_origduration\"");
            multipart.Add(requestbodyContent_origduration);
            // LOC DATE
            //var requestbodyContent_LOCDATE = new StringContent(videoEventModel.videoevent_modifylocdate);
            //requestbodyContent_LOCDATE.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_modifylocdate\"");
            //multipart.Add(requestbodyContent_LOCDATE);
            // Video Event Planning
            var requestbodyContent_PLANNING = new StringContent(videoEventModel.videoevent_planning.ToString());
            requestbodyContent_PLANNING.Headers.Add("Content-Disposition", "form-data; name=\"videoevent_planning\"");
            multipart.Add(requestbodyContent_PLANNING);

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

                var temp = PathHelper.GetTempPath("videoevent");
                pathWithFilename = $"{temp}\\{filename}";
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

        #endregion

        #region == Design Only ==
        public async Task<DesignModel> PutDesign(Int64 serverDesignId, Int64 selectedServerVideoEventId, int fk_design_screen, string design_xml)
        {
            try
            {
                var url = $"api/connect/videoevent/{selectedServerVideoEventId}/design/{serverDesignId}";
                var parameters = new Dictionary<string, string>
                {
                    { "fk_design_screen", fk_design_screen.ToString() },
                    { "design_xml", design_xml.ToString() }
                };
                var payload = new FormUrlEncodedContent(parameters);

                var result = await _apiClientHelper.Update<ParentData<DesignModel>>(url, payload);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.Message}");
                LogManagerHelper.WriteErroreLog($"Inside exception - {ex.InnerException}");
                return null;
            }
        }

        #endregion

        #region == VideoSegment Only ==

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

                var temp = PathHelper.GetTempPath("videosegment");
                pathWithFilename = $"{temp}\\{filename}";
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
        
        public async Task<ParentData<VideoSegmentPostResponse>> PutVideoSegment(Int64 videoevent_serverid, EnumMedia MediaType, byte[] blob = null)
        {
            var url = $"api/connect/videoevent/{videoevent_serverid}/videosegment-update";
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

                var temp = PathHelper.GetTempPath("videosegment");
                pathWithFilename = $"{temp}\\{filename}";
                var file = new FileStream(pathWithFilename, FileMode.OpenOrCreate, FileAccess.Write);
                file.Write(blob, 0, blob.Length);
                file.Close();

                fileReadStream = new FileStream(pathWithFilename, FileMode.Open);
                fileStreamContent = new StreamContent(fileReadStream);
                fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{filename}\"");
                multipart.Add(fileStreamContent);
            }

            var result = await _apiClientHelper.CreateWithMultipart<ParentData<VideoSegmentPostResponse>>(url, multipart);
            if (pathWithFilename?.Length > 0)
            {

                fileReadStream?.Close();
                fileStreamContent.Dispose();
                File.Delete(pathWithFilename);
            }
            return result;
        }

        #endregion

        #region == Notes Only ==

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

        public async Task<List<NotesResponseModel>> PUTNotes(Int64 selectedServerVideoEventId, List<NotesModelPut> notes)
        {
            var url = $"api/connect/videoevent/{selectedServerVideoEventId}/notes";
            var parameters = new Dictionary<string, string>
            {
                { "notes", JsonConvert.SerializeObject(notes) }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var response = await _apiClientHelper.Update<ParentData<List<NotesResponseModel>>>(url, payload);
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

        #endregion

        public async Task<string> DownloadBackground(int backgroundId, string extension)
        {
            string result = Path.GetTempPath();
            Console.WriteLine(result);


            var url = $"api/connect/background/download/{backgroundId}";
            var filename = $"background_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            var success = await _apiClientHelper.GetFile(url, filename);
            if (success)
            {
                var filepath = $@"{PathHelper.GetTempPath("background")}\\{filename}";
                return filepath;
            }
            return null;
        }

        public async Task<byte[]> GetSecuredFileByteArray(string url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            var result = await _apiClientHelper.GetSecuredFileByteArray(url);
            return result;
        }



    }
}
