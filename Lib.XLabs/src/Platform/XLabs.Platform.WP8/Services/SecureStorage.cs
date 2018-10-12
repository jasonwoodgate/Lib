// ***********************************************************************
// Assembly         : XLabs.Platform.WinUniversal
// Author           : XLabs Team
// Created          : 01-01-2016
// 
// Last Modified By : XLabs Team
// Last Modified On : 01-04-2016
// ***********************************************************************
// <copyright file="SecureStorage.cs" company="XLabs Team">
//     Copyright (c) XLabs Team. All rights reserved.
// </copyright>
// <summary>
//       This project is licensed under the Apache 2.0 license
//       https://github.com/XLabs/Xamarin-Forms-Labs/blob/master/LICENSE
//       
//       XLabs is a open source project that aims to provide a powerfull and cross 
//       platform set of controls tailored to work with Xamarin Forms.
// </summary>
// ***********************************************************************
// 

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Buffer = Windows.Storage.Streams.Buffer;

namespace XLabs.Platform.Services
{
    /// <summary>
    /// Implements <see cref="ISecureStorage"/> for WP using IsolatedStorageFile and ProtectedData
    /// </summary>
    public class SecureStorage : ISecureStorage
    {
        private static Windows.Storage.ApplicationData AppStorage { get {  return ApplicationData.Current; } }

        //private static readonly DataProtectionProvider _dataProtectionProvider = new DataProtectionProvider();

        private readonly byte[] _optionalEntropy;

        /// <summary>
        /// Initializes a new instance of <see cref="SecureStorage"/>.
        /// </summary>
        /// <param name="optionalEntropy">Optional password for additional entropy to make encyption more complex.</param>
        public SecureStorage(byte[] optionalEntropy)
        {
            this._optionalEntropy = optionalEntropy;					}

        /// <summary>
        /// Initializes a new instance of <see cref="SecureStorage"/>.
        /// </summary>
        public SecureStorage() : this(null)
        {
            
        }

        #region ISecureStorage Members

        /// <summary>
        /// Stores the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="dataBytes">The data bytes.</param>
        public async void Store(string key, byte[] dataBytes)
        {
            var mutex = new Mutex(false, key);

            try
            {
                mutex.WaitOne();

                //var result = await new Windows.Security.Cryptography.DataProtection.DataProtectionProvider().ProtectAsync()

                var buffer = dataBytes.AsBuffer();

                //if (_optionalEntropy != null)
                //{
                //    buffer = await _dataProtectionProvider.ProtectAsync(buffer);
                //}

                var file =
                    await AppStorage.LocalFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

                var stream = await file.OpenStreamForWriteAsync();
                var byteArray = buffer.ToArray();

                await stream.WriteAsync(byteArray, 0, byteArray.Length);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Retrieves the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System.Byte[].</returns>
        /// <exception cref="System.Exception"></exception>
        public byte[] Retrieve(string key)
        {
            var mutex = new Mutex(false, key);

            try
            {
                mutex.WaitOne();

                var task = Task.Factory.StartNew(async () =>
                                                     {
                                                         var file = await AppStorage.LocalFolder.GetFileAsync(key);
                                                         var stream = await file.OpenReadAsync();
                                                         var buff = new Buffer(10240);
                                                         IBuffer bufferResult;
                                                         var byteResult = new List<byte>();

                                                         do
                                                         {
                                                             bufferResult =
                                                                 await
                                                                     stream.ReadAsync(buff, 10240,
                                                                         InputStreamOptions.None);

                                                             if (bufferResult.Length > 0)
                                                             {
                                                                 byteResult.AddRange(bufferResult.ToArray());
                                                             }
                                                         } while (bufferResult.Length >= stream.Size);


                                                         //if (_optionalEntropy != null)
                                                         //{
                                                         //    byteResult =
                                                         //        _dataProtectionProvider.UnprotectAsync(byteResult)
                                                         //                               .GetResults();
                                                         //}

                                                         return byteResult.ToArray();
                                                     });

                return task.Result.Result;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("No entry found for key {0}.", key), ex);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Deletes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public async void Delete(string key)
        {
            var mutex = new Mutex(false, key);

            try
            {
                mutex.WaitOne();

                var file = await AppStorage.LocalFolder.GetFileAsync(key);
                
                await file.DeleteAsync();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Checks if the storage contains a key.
        /// </summary>
        /// <param name="key">The key to search.</param>
        /// <returns>True if the storage has the key, otherwise false.</returns>
        public bool Contains(string key)
        {
            try
            {
                var file = AppStorage.LocalFolder.GetFileAsync(key);
                file.GetResults();

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
