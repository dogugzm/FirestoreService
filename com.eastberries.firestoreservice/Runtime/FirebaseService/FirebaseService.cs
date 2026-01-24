using System;
using Firebase.Firestore;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using VContainer;

namespace FirebaseService
{
    public class FirebaseService : IFirebaseService
    {
        [Inject] private readonly Assets.Scripts.LoadingService.LoadingService _loadingService;

        public async UniTask<FirebaseResult<List<T>>> GetDataFromCollectionAsync<T>(string collectionName)
        {
            try
            {
                _loadingService.StartUniTask();
                var snapshot = await FirebaseFirestore.DefaultInstance
                    .Collection(collectionName)
                    .GetSnapshotAsync();

                var result = new List<T>();

                foreach (var item in snapshot.Documents)
                {
                    var data = item.ConvertTo<T>();
                    result.Add(data);
                }

                return FirebaseResult<List<T>>.Success(result);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error fetching data from Firebase: {ex.Message}");
                return FirebaseResult<List<T>>.Failure($"Error fetching data from Firebase: {ex.Message}");
            }
            finally
            {
                _loadingService.CompleteUniTask();
            }
        }

        public async UniTask<FirebaseResult<T>> GetDataByIdAsync<T>(string collectionName, string id)
        {
            try
            {
                _loadingService.StartUniTask();

                var snapshot = await FirebaseFirestore.DefaultInstance
                    .Collection(collectionName)
                    .Document(id)
                    .GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return FirebaseResult<T>.Failure(
                        $"Document with ID {id} does not exist in collection {collectionName}.");
                }

                var data = snapshot.ConvertTo<T>();
                return FirebaseResult<T>.Success(data);
            }
            catch (Exception ex)
            {
                return FirebaseResult<T>.Failure($"Error fetching data from Firebase: {ex.Message}");
            }
            finally
            {
                _loadingService.CompleteUniTask();
            }
        }

        public async UniTask<FirebaseResult<bool>> SetDataAsync<T>(T data, string collectionName, string id)
        {
            try
            {
                _loadingService.StartUniTask();

                await FirebaseFirestore.DefaultInstance
                    .Collection(collectionName)
                    .Document(id)
                    .SetAsync(data);

                return FirebaseResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error setting data to Firebase: {ex.Message}");
                return FirebaseResult<bool>.Failure($"Error setting data to Firebase: {ex.Message}");
            }
            finally
            {
                _loadingService.CompleteUniTask();
            }
        }

        public async UniTask<FirebaseResult<bool>> CreateDataAsync<T>(T data, string collectionName)
        {
            try
            {
                _loadingService.StartUniTask();

                await FirebaseFirestore.DefaultInstance
                    .Collection(collectionName)
                    .AddAsync(data);

                return FirebaseResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating data on Firebase: {ex.Message}");
                return FirebaseResult<bool>.Failure($"Error creating data on Firebase: {ex.Message}");
            }
            finally
            {
                _loadingService.CompleteUniTask();
            }
        }

        public async UniTask<FirebaseResult<bool>> UpdateDataAsync(string collectionName, string id,
            Dictionary<string, object> newData)
        {
            try
            {
                _loadingService.StartUniTask();

                await FirebaseFirestore.DefaultInstance
                    .Collection(collectionName)
                    .Document(id)
                    .UpdateAsync(newData);


                return FirebaseResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error updating data on Firebase: {ex.Message}");
                return FirebaseResult<bool>.Failure($"Error updating data on Firebase: {ex.Message}");
            }
            finally
            {
                _loadingService.CompleteUniTask();
            }
        }
    }
}