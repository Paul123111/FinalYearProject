// Copyright 2019 Google LLC
// All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Agones;
using UnityEngine;
using Mirror;
using TMPro;

namespace AgonesExample {
    [RequireComponent(typeof(AgonesAlphaSdk))]
    public class AgonesServer : MonoBehaviour {
        
        private AgonesAlphaSdk agones = null;

        async void Start() {
            agones = GetComponent<AgonesAlphaSdk>();
            bool ok = await agones.Connect();
            if (ok) {
                Debug.Log(("Server - Connected"));
            } else {
                Debug.Log(("Server - Failed to connect, exiting"));
                Application.Quit(1);
            }

            ok = await agones.Ready();
            await agones.SetPlayerCapacity(4);
            if (ok) {
                Debug.Log($"Server - Ready");
            } else {
                Debug.Log($"Server - Ready failed");
                Application.Quit();
            }
        }

        //void OnDestroy() {
        //    //bool ok = await agones.Shutdown();
        //    //if (ok) {
        //    //    Debug.Log("Server - Close");
        //    //} else {
        //    //    Debug.Log("Server did not shutdown properly");
        //    //}
        //}
    }
}