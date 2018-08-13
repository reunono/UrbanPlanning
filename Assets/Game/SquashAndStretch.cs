using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace Gameloft.FiftyDudes
{
	/// <summary>
	/// Add this component to an object and it'll squash and stretch when moving around.
	/// IMPORTANT :
	/// You need to have the following hierarchy structure :
	/// - Top level object (rigidbody, controller, etc)
	///   - This script on an empty game object
	///     - your model, renderer, material, mesh etc
	/// If you've got questions ask Alejandro or Renaud (in that order)
	/// </summary>
	public class SquashAndStretch : MonoBehaviour 
	{
		public float SquashAmount;
		public float MaxSquashAmount = 3.0f;
		public float MaxSquashSpeed = 200.0f;
        public bool BasedOnPosition;
        public float PositionFactor = 100f;

		protected Transform _transform;
		protected Transform _childTransform;
		protected Transform _parentTransform;
		protected Vector3 _previousPosition;
		protected Rigidbody _parentRigidbody;
		protected Vector3 _squash;

		/// <summary>
		/// On start we grab our various components and store them for future use
		/// </summary>
		protected virtual void Start () 
		{
			_transform = this.transform;
			_childTransform = _transform.GetChild (0).transform;
			_parentRigidbody = _transform.parent.GetComponent<Rigidbody> ();
			_parentTransform = _transform.parent.GetComponent<Transform> ();
		}

		/// <summary>
		/// Every frame we scale our object based on its current velocity
		/// </summary>
		protected virtual void LateUpdate () 
		{
			float dt = Time.deltaTime;
            Vector3 currPosition = _transform.position;
            Vector3 velocity = new Vector3();

            if (BasedOnPosition)
            {
                velocity = (_previousPosition - currPosition) * PositionFactor;
            }
            else
            {
                velocity = _parentRigidbody.velocity;
            }

            velocity.z = 0.0f;
            Vector3 velocityDir = Vector3.Normalize(velocity);
            float vMag = velocity.magnitude;
            float squashLerp = vMag / MaxSquashSpeed;

            Quaternion targetRotation = Quaternion.identity;
			if (vMag > 0.01f) 
			{
				targetRotation = Quaternion.FromToRotation (Vector3.up, velocityDir);
			}
			Quaternion deltaRotation = _parentTransform.rotation;
			_transform.rotation = targetRotation;
			_childTransform.rotation = deltaRotation;
			_transform.localScale = Vector3.one;

			float squash_y = Mathf.Clamp (vMag, 1.0f, MaxSquashAmount);
			float squash_xz = Mathf.Clamp01(1.0f / (vMag + 0.001f));
			_squash.x = squash_xz;
			_squash.y = squash_y;
			_squash.z = squash_xz;

			_squash = Vector3.Lerp (Vector3.one, _squash, squashLerp * SquashAmount);
			_transform.localScale = _squash;
			_previousPosition = currPosition;
		}
	}
}