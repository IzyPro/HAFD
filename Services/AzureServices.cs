using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HAFD.Data;
using HAFD.Enums;
using HAFD.Helpers;
using HAFD.Models;
using HAFD.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace HAFD.Services
{
    public interface IAzureService
    {
        Task<ResponseManager> AddPersonAsync(RegisterUserViewModel model, IFormFileCollection images);
        Task<ResponseManager> TrainGroupAsync();
        Task<Tuple<ResponseManager, List<User>>> IdentifyFacesAsync(IFormFileCollection model);
    }
    public class AzureServices : IAzureService
    {
        private FaceClient faceClient;
        private IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        private DatabaseContext _context;
        private IUserServices _userServices;
        Guid _filename = Guid.NewGuid();

        public AzureServices(IConfiguration configuration, IWebHostEnvironment hostEnvironment, DatabaseContext context, IUserServices userServices)
        {
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
            _userServices = userServices;
            InitFaceClient();
            _context = context;
        }
        //ADD PERSON TO LARGE PERSON GROUP
        public async Task<ResponseManager> AddPersonAsync(RegisterUserViewModel model, IFormFileCollection images)
        {
            try
            {
                //Check if User Already exists
                var existingUser = _context.Users.Where(x => x.Email.ToLower() == model.Email.ToLower()).FirstOrDefault();
                if (existingUser != null)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "User already exists",
                    };
                }
                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var personGroup = await faceClient.PersonGroup.GetAsync(personGroupId);
                var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
                if (personGroup == null)
                {
                    //Create new Large person group if it doesn't exist 
                    await faceClient.PersonGroup.CreateAsync(personGroupId, recognitionModel: recognitionModel, name: "TestGroup");
                }

                List<DetectedFace> detectedFaces = await DetectFace(faceClient, images, recognitionModel);
                if (detectedFaces == null)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "No face detected in the image(s)",
                    };
                }
                //Create person
                Person person = await faceClient.PersonGroupPerson.CreateAsync(personGroupId: personGroupId, name: model.Email, userData: model.Email);

                if (person == null)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to create user profile. Try Again",
                    };
                }
                string path = "";
                // Add face to the person group person.
                foreach (var faceImage in images)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    path = Path.Combine(wwwRootPath, $"images\\{_filename}.png");
                    using (var stream = new FileStream(path, FileMode.Open))
                    {
                        PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId: personGroupId, personId: person.PersonId, image: stream);
                    }
                    //using (var memoryStream = new MemoryStream())
                    //{
                    //    await faceImage.CopyToAsync(memoryStream);
                    //    using (var img = System.Drawing.Image.FromStream(memoryStream, true))
                    //    {
                    //        string wwwRootPath = _hostEnvironment.WebRootPath;
                    //        path = Path.Combine(wwwRootPath, $"images\\{_fi}.png");
                    //        img.Save(path);
                    //        using (var stream = new FileStream(path, FileMode.Open))
                    //        {
                    //            PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId: personGroupId, personId: person.PersonId, image: imageStream);
                    //        }
                    //    }
                    //}



                    //string wwwRootPath = _hostEnvironment.WebRootPath;
                    //string fileName = Path.GetFileNameWithoutExtension(faceImage.FileName);
                    //string extension = Path.GetExtension(faceImage.FileName);
                    //fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    //path = Path.Combine(wwwRootPath, $"images\\{fileName}");
                    //using (var fileStream = new FileStream(path, FileMode.Create))
                    //{
                    //    await faceImage.CopyToAsync(fileStream);
                    //    using (Stream imageStream = File.OpenRead(path))
                    //    {
                    //        PersistedFace face = await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId: personGroupId, personId: person.PersonId, image: imageStream);
                    //    }
                    //}
                }

                var user = new User
                {
                    Id = person.PersonId.ToString(),
                    Firstname = model.Firstname,
                    Lastname = model.Lastname,
                    Email = model.Email,
                    UserName = model.Email,
                    Department = model.Department,
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    Image = Base64Converter.ToBase64(path),
                    UserStatus = UserStatusEnum.Active,
                    DateCreated = DateTime.Now,
                };

                TrainGroupAsync();

                return await _userServices.RegisterUserAsync(model, user);
            }
            catch (Exception ex)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
            }
        }



        //TRAIN PERSON GROUP
        public async Task<ResponseManager> TrainGroupAsync()
        {
            try
            {
                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var personGroup = await faceClient.PersonGroup.GetAsync(personGroupId);
                if (personGroup == null)
                {
                    return new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to find person group. First, create a group by adding a person",
                    };
                }
                await faceClient.PersonGroup.TrainAsync(personGroupId);

                // Wait until the training is completed.
                while (true)
                {
                    await Task.Delay(1000);
                    var trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
                    if (trainingStatus.Status == TrainingStatusType.Succeeded)
                    {
                        return new ResponseManager
                        {
                            isSuccess = true,
                            Message = "Training completed",
                        };
                    }
                    else if (trainingStatus.Status == TrainingStatusType.Failed)
                    {
                        return new ResponseManager
                        {
                            isSuccess = false,
                            Message = "Training Failed. Try again",
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
            }
        }




        //IDENTIFY FACES
        public async Task<Tuple<ResponseManager, List<User>>> IdentifyFacesAsync(IFormFileCollection model)
        {
            try
            {
                var response = new ResponseManager();

                var personGroupId = _configuration["AzureDetails:PersonGroupID"];
                var recognitionModel = _configuration["AzureDetails:RecognitionModel"];
                var identifiedPersons = new List<User>();

                List<Guid> sourceFaceIds = new List<Guid>();

                var personGroup = await faceClient.PersonGroup.GetAsync(personGroupId);
                if (personGroup == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to find person group. First, create a group by adding a person",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                //Detect faces in image
                List<DetectedFace> detectedFaces = await DetectFace(faceClient, model, recognitionModel);
                if (detectedFaces == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "No face detected in the image(s)",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                // Add detected faceId to sourceFaceIds.
                foreach (var detectedFace in detectedFaces)
                {
                    sourceFaceIds.Add(detectedFace.FaceId.Value);
                }

                var identifyResults = await faceClient.Face.IdentifyAsync(sourceFaceIds, personGroupId: personGroupId);

                if (identifyResults == null)
                {
                    response = new ResponseManager
                    {
                        isSuccess = false,
                        Message = "Unable to identify persons. User does not exist.",
                    };
                    return new Tuple<ResponseManager, List<User>>(response, null);
                }

                foreach (var identifyResult in identifyResults)
                {
                    if (identifyResult.Candidates.Count > 0)
                    {
                        Person person = await faceClient.PersonGroupPerson.GetAsync(personGroupId, identifyResult.Candidates[0].PersonId);
                        var confidence = identifyResult.Candidates[0].Confidence;

                        if (confidence >= 0.5)
                        {
                            //var user = _context.Users.Where(x => x.Id == person.PersonId.ToString()).FirstOrDefault();
                            var user = await _context.Users.Include(x => x.Hostel).FirstOrDefaultAsync(x => x.Email == person.Name);
                            identifiedPersons.Add(user);
                        }
                    }
                }

                response = new ResponseManager
                {
                    isSuccess = true,
                    Message = "Identification Successful",
                };
                return new Tuple<ResponseManager, List<User>>(response, identifiedPersons);
            }
            catch (Exception ex)
            {
                var response = new ResponseManager
                {
                    isSuccess = false,
                    Message = ex.Message,
                };
                return new Tuple<ResponseManager, List<User>>(response, null);
            }
        }




        //DETECT FACES IN IMAGES
        public async Task<List<DetectedFace>> DetectFace(IFaceClient faceClient, IFormFileCollection image, string recognition_model)
        {
            try
            {
                IList<DetectedFace> detectedFaces = null;
                List<DetectedFace> allDetectedFaces = new List<DetectedFace>();
                // We use detection model 3 because we are not retrieving attributes.
                foreach (var file in image)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await file.CopyToAsync(memoryStream);
                        using (var img = System.Drawing.Image.FromStream(memoryStream, true))
                        {
                            string wwwRootPath = _hostEnvironment.WebRootPath;
                            string path = Path.Combine(wwwRootPath, $"images\\{_filename}.png");
                            img.Save(path);
                            using (var stream = new FileStream(path, FileMode.Open))
                            {
                                detectedFaces = await faceClient.Face.DetectWithStreamAsync(stream, recognitionModel: recognition_model, detectionModel: DetectionModel.Detection03);
                                if (detectedFaces.Count < 1)
                                    return null;
                                foreach (var face in detectedFaces)
                                {
                                    allDetectedFaces.Add(face);
                                }
                            }
                        }
                    }
                }
                if (allDetectedFaces.Count < 1)
                    return null;
                return allDetectedFaces;
            }

            catch (Exception)
            {
                return null;
            }
        }


        void InitFaceClient()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(_configuration["AzureDetails:Key"]);
            faceClient = new FaceClient(credentials);
            faceClient.Endpoint = _configuration["AzureDetails:Endpoint"];
            FaceOperations faceOperations = new FaceOperations(faceClient);
        }
    }
}
