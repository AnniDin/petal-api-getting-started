using LSL;
using System.Linq;
using System;
using UnityEngine;
using System.Collections;

namespace Assets.LSL4Unity.Scripts.AbstractInlets
{
    public abstract class MyAFloatInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        float[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type ", StreamType);
            }

            StartCoroutine(MyResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }

        IEnumerator MyResolveExpectedStream()
        {

            Debug.Log("MY COROUTINE");
            var results = resolver.results();

            yield return new WaitUntil(() => results.Length > 0);

            Debug.Log(string.Format("Resolving Stream: {0}", StreamName));

            inlet = new liblsl.StreamInlet(results[0]);

            expectedChannels = inlet.info().channel_count();

            yield return null;
        }

        protected void pullSamples()
        {
            sample = new float[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(float[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }

    public abstract class MyADoubleInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        double[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type", StreamType);
            }

            StartCoroutine(ResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();

            while (inlet == null)
            {

                yield return new WaitUntil(() => results.Length > 0);

                inlet = new liblsl.StreamInlet(GetStreamInfoFrom(results));

                expectedChannels = inlet.info().channel_count();
            }

            yield return null;
        }

        private liblsl.StreamInfo GetStreamInfoFrom(liblsl.StreamInfo[] results)
        {
            var targetInfo = results.Where(r => r.name().Equals(StreamName)).First();
            return targetInfo;
        }

        protected void pullSamples()
        {
            sample = new double[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(double[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }

    public abstract class MyACharInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        char[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type", StreamType);
            }

            StartCoroutine(ResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }


        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();

            yield return new WaitUntil(() => results.Length > 0);

            inlet = new liblsl.StreamInlet(results[0]);

            expectedChannels = inlet.info().channel_count();

            yield return null;
        }

        protected void pullSamples()
        {
            sample = new char[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(char[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }

    public abstract class MyAShortInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        short[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type", StreamType);
            }

            StartCoroutine(ResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();

            yield return new WaitUntil(() => results.Length > 0);

            inlet = new liblsl.StreamInlet(results[0]);

            expectedChannels = inlet.info().channel_count();

            yield return null;
        }

        protected void pullSamples()
        {
            sample = new short[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(short[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }

    public abstract class MyAIntInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        int[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type", StreamType);
            }

            StartCoroutine(ResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();

            yield return new WaitUntil(() => results.Length > 0);

            inlet = new liblsl.StreamInlet(results[0]);

            expectedChannels = inlet.info().channel_count();

            yield return null;
        }

        protected void pullSamples()
        {
            sample = new int[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(int[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }

    public abstract class MyAStringInlet : MonoBehaviour
    {
        public enum UpdateMoment { FixedUpdate, Update }

        public UpdateMoment moment;

        public string StreamName;

        public string StreamType;

        liblsl.StreamInfo[] results;
        liblsl.StreamInlet inlet;
        liblsl.ContinuousResolver resolver;

        private int expectedChannels = 0;

        string[] sample;

        void Start()
        {
            var expectedStreamHasAName = !StreamName.Equals("");
            var expectedStreamHasAType = !StreamType.Equals("");

            if (!expectedStreamHasAName && !expectedStreamHasAType)
            {
                Debug.LogError("Inlet has to specify a name or a type before it is able to lookup a stream.");
                this.enabled = false;
                return;
            }

            if (expectedStreamHasAName)
            {
                Debug.Log("Creating LSL resolver for stream " + StreamName);

                resolver = new liblsl.ContinuousResolver("name", StreamName);
            }
            else if (expectedStreamHasAType)
            {
                Debug.Log("Creating LSL resolver for stream with type " + StreamType);
                resolver = new liblsl.ContinuousResolver("type", StreamType);
            }

            StartCoroutine(ResolveExpectedStream());

            AdditionalStart();
        }
        /// <summary>
        /// Override this method in the subclass to specify what should happen during Start().
        /// </summary>
        protected virtual void AdditionalStart()
        {
            //By default, do nothing.
        }

        IEnumerator ResolveExpectedStream()
        {
            var results = resolver.results();

            yield return new WaitUntil(() => results.Length > 0);

            inlet = new liblsl.StreamInlet(results[0]);

            expectedChannels = inlet.info().channel_count();

            yield return null;
        }

        protected void pullSamples()
        {
            sample = new string[expectedChannels];

            try
            {
                double lastTimeStamp = inlet.pull_sample(sample, 0.0f);

                if (lastTimeStamp != 0.0)
                {
                    // do not miss the first one found
                    Process(sample, lastTimeStamp);
                    // pull as long samples are available
                    while ((lastTimeStamp = inlet.pull_sample(sample, 0.0f)) != 0)
                    {
                        Process(sample, lastTimeStamp);
                    }

                }
            }
            catch (ArgumentException aex)
            {
                Debug.LogError("An Error on pulling samples deactivating LSL inlet on...", this);
                this.enabled = false;
                Debug.LogException(aex, this);
            }

        }

        /// <summary>
        /// Override this method in the subclass to specify what should happen when samples are available.
        /// </summary>
        /// <param name="newSample"></param>
        protected abstract void Process(string[] newSample, double timeStamp);

        void FixedUpdate()
        {
            if (moment == UpdateMoment.FixedUpdate && inlet != null)
                pullSamples();
        }

        void Update()
        {
            if (moment == UpdateMoment.Update && inlet != null)
                pullSamples();
        }
    }
}
